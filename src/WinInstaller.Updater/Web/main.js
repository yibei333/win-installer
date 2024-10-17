function getIcon() {
    return window.external.GetIcon();
}

function GetCurrentVersion() {
    return window.external.GetCurrentVersion()
}

function checkUpdate() {
    window.external.CheckUpdate();
}

function downloadUpdate() {
    window.external.DownloadAndUpdate();
}

function cancleDownloadAndUpdate() {
    window.external.StopUpdate();
}