function menuActive(show, active,parent) {
    show.addClass("show");
    active.addClass("active");
    parent.parent().addClass("active");
}
function menuParentActive(active) {
    active.parent().addClass("active");
}