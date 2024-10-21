var viewModel = {
    panelHeight: ko.observable(''),
    installLocation: ko.observable(GetInstallLocation()),
    running: ko.observable(false),
    log: ko.observable(""),
    success: ko.observable(false),
    uninstall: function () {
        setRunningState(true);
        UnInstall();
    },
    close: function () {
        Close();
    }
};

function setLog(value) {
    viewModel.log(value);
    setPaddingTop();
}

function setRunningState(value) {
    viewModel.running(value);
    setPaddingTop();
}

function setSuccessState(value) {
    viewModel.success(value);
    setPaddingTop();
}

function setPaddingTop() {
    var main = document.getElementById("app");
    var mainHeight = main.clientHeight;
    var mainPanel = document.getElementById("main-panel");
    var panelHeight = mainPanel.clientHeight;
    viewModel.panelHeight((mainHeight - panelHeight) / 2 + 'px');
}

window.onload = function () {
    ko.applyBindings(viewModel, document.getElementById('app'));
    setPaddingTop();

    window.addEventListener('mousewheel', function (event) {
        if (event.ctrlKey === true || event.metaKey) {
            event.preventDefault();
        }
    }, { passive: false });

    //firefox
    window.addEventListener('DOMMouseScroll', function (event) {
        if (event.ctrlKey === true || event.metaKey) {
            event.preventDefault();
        }
    }, { passive: false });
}
