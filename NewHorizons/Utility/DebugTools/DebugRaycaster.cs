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

        private GameObject _planeUpRightSphere;
        private GameObject _planeUpLeftSphere;
        private GameObject _planeDownRightSphere;
        private GameObject _planeDownLeftSphere;

        private ScreenPrompt _raycastPrompt;
        private ScreenPrompt _airbornePrompt;
        private ScreenPrompt _airborneZPrompt;

        private static readonly int defaultZ = 4;
        private int _z = defaultZ;

        private bool isAirborneMode;

        public void Start()
        {
            isAirborneMode = false;
            _rb = this.GetRequiredComponent<OWRigidbody>();

            if (_raycastPrompt == null)
            {
                _raycastPrompt = new ScreenPrompt(TranslationHandler.GetTranslation("DEBUG_RAYCAST", TranslationHandler.TextType.UI) + " <CMD>", ImageUtilities.GetButtonSprite(KeyCode.P));
                Locator.GetPromptManager().AddScreenPrompt(_raycastPrompt, PromptPosition.UpperRight, false);
            }

            if (_airbornePrompt == null)
            {
                _airbornePrompt = new ScreenPrompt(TranslationHandler.GetTranslation("DEBUG_AIRBORNE", TranslationHandler.TextType.UI) + " <CMD>", ImageUtilities.GetButtonSprite(KeyCode.O));
                Locator.GetPromptManager().AddScreenPrompt(_airbornePrompt, PromptPosition.UpperRight, false);
            }

            if (_airborneZPrompt == null)
            {
                _airborneZPrompt = new MultiButtonScreenPrompt(TranslationHandler.GetTranslation("DEBUG_AIRBORNE_Z", TranslationHandler.TextType.UI) + " <CMD1> <CMD2>", ImageUtilities.GetButtonSprite(KeyCode.K), ImageUtilities.GetButtonSprite(KeyCode.I));
                Locator.GetPromptManager().AddScreenPrompt(_airborneZPrompt, PromptPosition.UpperRight, false);
            }
        }

        public void OnDestroy()
        {
            if (_raycastPrompt != null)
            {
                Locator.GetPromptManager()?.RemoveScreenPrompt(_raycastPrompt, PromptPosition.UpperRight);
            }
            if (_airbornePrompt != null)
            {
                Locator.GetPromptManager()?.RemoveScreenPrompt(_airbornePrompt, PromptPosition.UpperRight);
            }
            if (_airborneZPrompt != null)
            {
                Locator.GetPromptManager()?.RemoveScreenPrompt(_airborneZPrompt, PromptPosition.UpperRight);
            }
        }

        public void Update()
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
                    _z += 1;
                    UpdateAirborneTarget();
                    PlayAudio(AudioType.Menu_ChangeTab);
                }
                else if (Keyboard.current[Key.K].wasReleasedThisFrame)
                {
                    _z -= 1;
                    UpdateAirborneTarget();
                    PlayAudio(AudioType.Menu_ChangeTab);
                }
            }
        }

        public void ToggleMode()
        {
            PlayAudio(AudioType.Menu_ChangeTab);
            _z = defaultZ;
            if (isAirborneMode)
            {
                isAirborneMode = false;
                Destroy(_airborneTarget);
            } else
            {
                isAirborneMode = true;
                _airborneTarget = AddDebugShape.AddSphere(Locator.GetActiveCamera().gameObject, 0.5f, Color.red);
                UpdateAirborneTarget();
            }
        }

        public void UpdateAirborneTarget()
        {
            if (_airborneTarget != null)
            {
                _airborneTarget.transform.localPosition = Vector3.forward * _z;
            }
        }

        public void UpdatePromptVisibility()
        {
            if (_raycastPrompt != null)
            {
                _raycastPrompt.SetVisibility(!OWTime.IsPaused() && Main.Debug);
            }
            if (_airbornePrompt != null)
            {
                _airbornePrompt.SetVisibility(!OWTime.IsPaused() && Main.Debug);
            }
            if (_airborneZPrompt != null)
            {
                _airborneZPrompt.SetVisibility(!OWTime.IsPaused() && Main.Debug && isAirborneMode);
            }
        }

        internal string Vector3ToString(Vector3 v) => $"{{\"x\": {v.x}, \"y\": {v.y}, \"z\": {v.z}}}";

        private void DestroyDebugSpheres()
        {
            if (_surfaceSphere != null) Destroy(_surfaceSphere);
            if (_normalSphere1 != null) Destroy(_normalSphere1);
            if (_normalSphere2 != null) Destroy(_normalSphere2);
            if (_planeUpRightSphere != null) Destroy(_planeUpRightSphere);
            if (_planeUpLeftSphere != null) Destroy(_planeUpLeftSphere);
            if (_planeDownLeftSphere != null) Destroy(_planeDownLeftSphere);
            if (_planeDownRightSphere != null) Destroy(_planeDownRightSphere);
        }

        internal void PrintRaycast()
        {
            DebugRaycastData data = Raycast();

            if (!data.hit)
            {
                NHLogger.LogWarning(
                    isAirborneMode ? "Airborne raycast failed because player is not in a sector!" :
                    "Debug Raycast Didn't Hit Anything! (Try moving closer)"
                );
                return;
            }

            var posText = Vector3ToString(data.pos);
            var normText = Vector3ToString(data.norm);
            var rotText = Vector3ToString(data.rot.eulerAngles);

            DestroyDebugSpheres();

            _surfaceSphere = AddDebugShape.AddSphere(data.hitBodyGameObject, 0.1f, Color.green);
            _surfaceSphere.transform.localPosition = data.pos;

            if (!isAirborneMode)
            {
                _normalSphere1 = AddDebugShape.AddSphere(data.hitBodyGameObject, 0.01f, Color.red);
                _normalSphere2 = AddDebugShape.AddSphere(data.hitBodyGameObject, 0.01f, Color.red);

                _normalSphere1.transform.localPosition = data.pos + data.norm * 0.5f;
                _normalSphere2.transform.localPosition = data.pos + data.norm;
            }

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

            if (!isAirborneMode)
            {
                NHLogger.Log($"Raycast hit\n\n\"position\": {posText},\n\"rotation\": {rotText},\n\"normal\": {normText}\n\non collider [{data.colliderPath}] " +
                           (data.bodyPath != null ? $"at rigidbody [{data.bodyPath}]" : "not attached to a rigidbody"));
            }
            else
            {
                NHLogger.Log($"Airborne Raycast hit\n\n\"position\": {posText},\n\"rotation\": {rotText},\n\non sector [{data.colliderPath}] " +
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
                var rotation = Locator.GetActiveCamera().transform.rotation;
                var direction = Locator.GetActiveCamera().transform.forward;

                if (isAirborneMode)
                {
                    var currentSector = Locator.GetPlayerSectorDetector().GetLastEnteredSector();
                    data.hit = currentSector != null;
                    if (data.hit)
                    {
                        var currentRigidbody = currentSector.GetAttachedOWRigidbody();

                        data.pos = currentRigidbody.transform.InverseTransformPoint(_airborneTarget.transform.position);
                        data.norm = currentRigidbody.transform.InverseTransformDirection(direction);
                        data.rot = currentRigidbody.transform.InverseTransformRotation(rotation);

                        data.colliderPath = currentSector.transform.GetPath();
                        data.bodyPath = currentRigidbody.transform.GetPath();
                        data.hitBodyGameObject = currentRigidbody.gameObject;
                        data.hitObject = currentSector.gameObject;

                        data.plane = ConstructPlane(data);
                    }
                }
                else
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

        public void PlayAudio(AudioType type) => Locator.GetPlayerAudioController().PlayOneShotInternal(type);
    }
}
