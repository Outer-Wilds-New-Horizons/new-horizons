$(document).on('click', 'a[href^="#"]', function (event) {
    if (this.href.split("#")[1] === "top") return;
    event.preventDefault();
    history.pushState({}, '', this.href);
});

function flashElement(elementId) {
    myElement = document.getElementById(elementId).parentNode;
    myElement.classList.add("jsfh-animated-property");
    setTimeout(function () {
        myElement.classList.remove("jsfh-animated-property");
    }, 1000);
}

function setAnchor(anchorLinkDestination) {
    history.pushState({}, '', anchorLinkDestination);
}

function anchorOnLoad() {
    let linkTarget = window.location.hash.split("?")[0].split("&")[0];
    if (linkTarget[0] === "#") {
        let idTarget = linkTarget.substring(1);
        if (idTarget !== "top") {
            anchorLink(idTarget);
        }
    }
}

function anchorLink(linkTarget) {
    const target = $("#" + linkTarget);
    target.parents().addBack().filter(".collapse:not(.show), .tab-pane, [role='tab']").each(
        function (index) {
            if ($(this).hasClass("collapse")) {
                $(this).collapse("show");
            } else if ($(this).hasClass("tab-pane")) {
                const tabToShow = $("a[href='#" + $(this).attr("id") + "']");
                if (tabToShow) {
                    tabToShow.tab("show");
                }
            } else if ($(this).attr("role") === "tab") {
                $(this).tab("show");
            }
        }
    );

    setTimeout(function () {
        let targetElement = document.getElementById(linkTarget);
        if (targetElement) {
            targetElement.scrollIntoView({block: "center", behavior: "smooth"});
            setTimeout(function () {
                flashElement(linkTarget);
            }, 500);
        }
    }, 1000);
}