function getIcon() {
    return window.external.GetIcon();
}

function GetCurrentVersion() {
    return window.external.GetCurrentVersion()
}

function CheckUpdate() {
    window.external.CheckUpdate();
}

function DownloadUpdate() {
    window.external.DownloadAndUpdate();
}

function CancleDownloadAndUpdate() {
    window.external.StopUpdate();
}