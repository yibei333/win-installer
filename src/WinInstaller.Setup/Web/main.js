var viewModel = {
    panelHeight: ko.observable(''),
    currentVersion: ko.observable(GetCurrentVersion()),
    lastVersion: ko.observable(""),
    running: ko.observable(false),
    progress_total: ko.observable(200),
    progress_handled: ko.observable(100),
    progress_progress: ko.observable(50),
    progress_speed: ko.observable('2.5MB/s'),
    hasUpdate: ko.pureComputed({
        read: function () {
            return viewModel.currentVersion() != '' && viewModel.lastVersion() != '' && viewModel.currentVersion() != viewModel.lastVersion();
        }
    }),
    alreadyUpdateToDate: ko.pureComputed({
        read: function () {
            return viewModel.currentVersion() != '' && viewModel.lastVersion() != '' && viewModel.currentVersion() == viewModel.lastVersion();
        }
    }),
    checkShow: ko.pureComputed({
        read: function () {
            return !viewModel.hasUpdate();
        }
    }),
    installShow: ko.pureComputed({
        read: function () {
            return viewModel.hasUpdate() && !viewModel.running();
        }
    }),
    check: function () {
        setRunningState(true);
        setLastVersion('');
        CheckUpdate();
    },
    update: function () {
        setProgress(0, 0, 0, '0')
        setRunningState(true);
        DownloadUpdate();
    },
    cancleUpdate: function () {
        CancleDownloadAndUpdate();
    }
};

function setLastVersion(value) {
    viewModel.lastVersion(value);
    setPaddingTop();
}

function setProgress(total, handled, progress, speed) {
    viewModel.progress_total(total);
    viewModel.progress_handled(handled);
    viewModel.progress_progress(progress);
    viewModel.progress_speed(speed);
    setPaddingTop();
}

function setRunningState(value) {
    viewModel.running(value);
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