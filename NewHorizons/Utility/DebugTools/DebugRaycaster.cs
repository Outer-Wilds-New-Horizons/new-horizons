using NewHorizons.Handlers;
using NewHorizons.Utility.Files;
using NewHorizons.Utility.Geometry;
using NewHorizons.Utility.OWML;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NewHorizons.Utility.DebugTools
{
    [RequireComponent(typeof(OWRigidbody))]
    public class DebugRaycaster : MonoBehaviour
    {
        private OWRigidbody _rb;
        private GameObject _surfaceSphere;
        private GameObject _normalSphere1;
        private GameObject _normalSphere2;
        private GameObject _airborneTarget;
        //private GameObject _airborneTargetSphere;

        private GameObject _planeUpRightSphere;
        private GameObject _planeUpLeftSphere;
        private GameObject _planeDownRightSphere;
        private GameObject _planeDownLeftSphere;

        private OWAudioSource _playerAudio;
        private ScreenPrompt _raycastPrompt;
        //private ScreenPrompt _switchToAirborne;

        private bool isAirborneMode;

        private void Start()
        {
            _playerAudio = SearchUtilities.Find("Player_Body/Audio_Player/OneShotAudio_Player").GetComponent<OWAudioSource>();
            isAirborneMode = false;
            _rb = this.GetRequiredComponent<OWRigidbody>();

            if (_raycastPrompt == null)
            {
                _raycastPrompt = new ScreenPrompt(TranslationHandler.GetTranslation("DEBUG_RAYCAST", TranslationHandler.TextType.UI) + " <CMD>", ImageUtilities.GetButtonSprite(KeyCode.P));
                Locator.GetPromptManager().AddScreenPrompt(_raycastPrompt, PromptPosition.UpperRight, false);
                //TODO: Figure out how to write screen prompts with 2 or more button sprites. P and O in this case.
            }
        }

        private void OnDestroy()
        {
            if (_raycastPrompt != null)
            {
                Locator.GetPromptManager()?.RemoveScreenPrompt(_raycastPrompt, PromptPosition.UpperRight);
            }
        }

        private void Update()
        {
            UpdatePromptVisibility();

            if (!Main.Debug) return;

            if (Keyboard.current == null) return;

            if (Keyboard.current[Key.O].wasReleasedThisFrame)
            {
                ToggleMode();
            } 
            else if (Keyboard.current[Key.P].wasReleasedThisFrame)
            {
                PrintRaycast();
            }

            if (isAirborneMode)
            {
                if (Keyboard.current[Key.I].wasReleasedThisFrame)
                {
                    _airborneTarget.transform.position.Set(Locator.GetActiveCamera().transform.position.x, Locator.GetActiveCamera().transform.position.y, Locator.GetActiveCamera().transform.position.z + 1);
                    _playerAudio.PlayOneShot(AudioType.Menu_ChangeTab);
                } else if (Keyboard.current[Key.K].wasReleasedThisFrame)
                {
                    _airborneTarget.transform.position.Set(Locator.GetActiveCamera().transform.position.x, Locator.GetActiveCamera().transform.position.y, Locator.GetActiveCamera().transform.position.z + 1);
                    _playerAudio.PlayOneShot(AudioType.Menu_ChangeTab);
                }
            }
        }

        public void ToggleMode()
        {
            _playerAudio.PlayOneShot(AudioType.Menu_ChangeTab);
            if (isAirborneMode)
            {
                isAirborneMode = false;
                Destroy(_airborneTarget);
                //Destroy(_airborneTargetSphere);
            } else
            {
                isAirborneMode = true;
                _airborneTarget = new GameObject("AirborneTarget");
                _airborneTarget.transform.parent = Locator.GetPlayerTransform();
                _airborneTarget.transform.position = Locator.GetPlayerTransform().position;
                _airborneTarget.transform.rotation = Locator.GetPlayerTransform().rotation;
                _airborneTarget.transform.localPosition.Set(0, 0, 4);
                _airborneTarget = AddDebugShape.AddSphere(_airborneTarget, 0.5f, Color.red);
            }
        }

        public void UpdatePromptVisibility()
        {
            if (_raycastPrompt != null)
            {
                _raycastPrompt.SetVisibility(!OWTime.IsPaused() && Main.Debug);
            }
        }

        internal string Vector3ToString(Vector3 v) => $"{{\"x\": {v.x}, \"y\": {v.y}, \"z\": {v.z}}}";

        internal void PrintRaycast()
        {
            DebugRaycastData data = Raycast();

            if (!isAirborneMode && !data.hit)
            {
                NHLogger.LogWarning("Debug Raycast Didn't Hit Anything! (Try moving closer)");
                return;
            }

            if (isAirborneMode)
            {
                var posText = Vector3ToString(data.pos);
                var rotText = Vector3ToString(data.rot.eulerAngles);

                if (_surfaceSphere != null) Destroy(_surfaceSphere);
                if (_normalSphere1 != null) Destroy(_normalSphere1);
                if (_normalSphere2 != null) Destroy(_normalSphere2);
                if (_planeUpRightSphere != null) Destroy(_planeUpRightSphere);
                if (_planeUpLeftSphere != null) Destroy(_planeUpLeftSphere);
                if (_planeDownLeftSphere != null) Destroy(_planeDownLeftSphere);
                if (_planeDownRightSphere != null) Destroy(_planeDownRightSphere);

                _surfaceSphere = AddDebugShape.AddSphere(_airborneTarget, 0.1f, Color.green);
                _surfaceSphere.transform.localPosition = data.pos;

                // plane corners
                var planeSize = 0.5f;
                var planePointSize = 0.05f;
                _planeUpRightSphere = AddDebugShape.AddSphere(_airborneTarget, planePointSize, Color.green);
                _planeUpLeftSphere = AddDebugShape.AddSphere(_airborneTarget, planePointSize, Color.cyan);
                _planeDownLeftSphere = AddDebugShape.AddSphere(_airborneTarget, planePointSize, Color.blue);
                _planeDownRightSphere = AddDebugShape.AddSphere(_airborneTarget, planePointSize, Color.cyan);

                _planeUpRightSphere.transform.localPosition = data.plane.origin + data.plane.u * 1 * planeSize + data.plane.v * 1 * planeSize;
                _planeUpLeftSphere.transform.localPosition = data.plane.origin + data.plane.u * -1 * planeSize + data.plane.v * 1 * planeSize;
                _planeDownLeftSphere.transform.localPosition = data.plane.origin + data.plane.u * -1 * planeSize + data.plane.v * -1 * planeSize;
                _planeDownRightSphere.transform.localPosition = data.plane.origin + data.plane.u * 1 * planeSize + data.plane.v * -1 * planeSize;
                NHLogger.Log($"Airborne Raycast hit\n\n\"position\": {posText},\n\"rotation\": {rotText},\n\n" +
                    (data.bodyPath != null ? $"at rigidbody [{_airborneTarget.transform.parent.parent.GetPath()}]" : "not attached to a rigidbody"));
            } else
            {
                var posText = Vector3ToString(data.pos);
                var normText = Vector3ToString(data.norm);
                var rotText = Vector3ToString(data.rot.eulerAngles);

                if (_surfaceSphere != null) Destroy(_surfaceSphere);
                if (_normalSphere1 != null) Destroy(_normalSphere1);
                if (_normalSphere2 != null) Destroy(_normalSphere2);
                if (_planeUpRightSphere != null) Destroy(_planeUpRightSphere);
                if (_planeUpLeftSphere != null) Destroy(_planeUpLeftSphere);
                if (_planeDownLeftSphere != null) Destroy(_planeDownLeftSphere);
                if (_planeDownRightSphere != null) Destroy(_planeDownRightSphere);

                _surfaceSphere = AddDebugShape.AddSphere(data.hitBodyGameObject, 0.1f, Color.green);
                _normalSphere1 = AddDebugShape.AddSphere(data.hitBodyGameObject, 0.01f, Color.red);
                _normalSphere2 = AddDebugShape.AddSphere(data.hitBodyGameObject, 0.01f, Color.red);

                _surfaceSphere.transform.localPosition = data.pos;
                _normalSphere1.transform.localPosition = data.pos + data.norm * 0.5f;
                _normalSphere2.transform.localPosition = data.pos + data.norm;

                // plane corners
                var planeSize = 0.5f;
                var planePointSize = 0.05f;
                _planeUpRightSphere = AddDebugShape.AddSphere(data.hitBodyGameObject, planePointSize, Color.green);
                _planeUpLeftSphere = AddDebugShape.AddSphere(data.hitBodyGameObject, planePointSize, Color.cyan);
                _planeDownLeftSphere = AddDebugShape.AddSphere(data.hitBodyGameObject, planePointSize, Color.blue);
                _planeDownRightSphere = AddDebugShape.AddSphere(data.hitBodyGameObject, planePointSize, Color.cyan);

                _planeUpRightSphere.transform.localPosition = data.plane.origin + data.plane.u * 1 * planeSize + data.plane.v * 1 * planeSize;
                _planeUpLeftSphere.transform.localPosition = data.plane.origin + data.plane.u * -1 * planeSize + data.plane.v * 1 * planeSize;
                _planeDownLeftSphere.transform.localPosition = data.plane.origin + data.plane.u * -1 * planeSize + data.plane.v * -1 * planeSize;
                _planeDownRightSphere.transform.localPosition = data.plane.origin + data.plane.u * 1 * planeSize + data.plane.v * -1 * planeSize;

                NHLogger.Log($"Raycast hit\n\n\"position\": {posText},\n\"rotation\": {rotText},\n\"normal\": {normText}\n\non collider [{data.colliderPath}] " +
                           (data.bodyPath != null ? $"at rigidbody [{data.bodyPath}]" : "not attached to a rigidbody"));
            }

        }
        public DebugRaycastData Raycast()
        {
            var data = new DebugRaycastData();

            _rb.DisableCollisionDetection();
            try
            {
                int layerMask = OWLayerMask.physicalMask;
                var origin = Locator.GetActiveCamera().transform.position;
                var direction = Locator.GetActiveCamera().transform.forward;

                if (isAirborneMode)
                {
                    _airborneTarget.transform.parent = Locator.GetPlayerSectorDetector().GetLastEnteredSector().gameObject.transform;
                    data.pos = _airborneTarget.transform.localPosition;
                    data.rot = _airborneTarget.transform.localRotation;
                    _airborneTarget.transform.parent = Locator.GetActiveCamera().transform;
                    data.plane = ConstructPlane(data);
                } else
                {
                    data.hit = Physics.Raycast(origin, direction, out var hitInfo, 100f, layerMask);
                    if (data.hit)
                    {
                        data.pos = hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point);
                        data.norm = hitInfo.rigidbody.transform.InverseTransformDirection(hitInfo.normal);

                        var toOrigin = Vector3.ProjectOnPlane((origin - hitInfo.point).normalized, hitInfo.normal);
                        var worldSpaceRot = Quaternion.LookRotation(toOrigin, hitInfo.normal);
                        data.rot = hitInfo.rigidbody.transform.InverseTransformRotation(worldSpaceRot);

                        data.colliderPath = hitInfo.collider.transform.GetPath();
                        data.bodyPath = hitInfo.rigidbody.transform.GetPath();
                        data.hitBodyGameObject = hitInfo.rigidbody.gameObject;
                        data.hitObject = hitInfo.collider.gameObject;

                        data.plane = ConstructPlane(data);
                    }
                }
            }
            catch
            {
                // ignored
            }
            _rb.EnableCollisionDetection();

            return data;
        }

        internal DebugRaycastPlane ConstructPlane(DebugRaycastData data)
        {
            var U = data.pos - Vector3.zero; // U is the local "up" direction. the direction directly away from the center of the planet at this point. // pos is always relative to the body, so the body is considered to be at 0,0,0.
            var R = data.pos; // R is our origin point for the plane
            var N = data.norm.normalized; // N is the normal for this plane

            if (Vector3.Cross(U, N) == Vector3.zero) U = new Vector3(0, 0, 1);
            if (Vector3.Cross(U, N) == Vector3.zero) U = new Vector3(0, 1, 0); // if 0,0,1 was actually the same vector U already was (lol), try (0,1,0) instead

            NHLogger.LogVerbose("Up vector is " + U.ToString());

            // stackoverflow.com/a/9605695
            // I don't know exactly how this works, but I'm projecting a point that is located above the plane's origin, relative to the planet, onto the plane. this gets us our v vector
            var q = (2 * U) - R;
            var dist = Vector3.Dot(N, q);
            var v_raw = 2 * U - dist * N;
            var v = (R - v_raw).normalized;

            var u = Vector3.Cross(N, v);

            DebugRaycastPlane p = new DebugRaycastPlane()
            {
                origin = R,
                normal = N,
                u = u,
                v = v
            };
            return p;
        }
    }
}
