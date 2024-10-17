function GetCurrentVersion() {
    return "1.0.0";
}

function checkUpdate(){
    var timer = setTimeout(function () {
        clearTimeout(timer);
        setLastVersion('1.0.1');
        setRunningState(false);
    }, 1000);
}

var cancleFlag = false;

function downloadUpdate() {
    cancleFlag = false;
    var total = 1000;
    var handled = 0;
    var timer = setInterval(function () {
        if (cancleFlag) {
            clearInterval(timer);
            setRunningState(false);
            return; 
        }

        if (handled >= total) {
            clearInterval(timer);
            setRunningState(false);
            return;
        }

        handled += 10;
        setProgress(total, handled, (handled * 100.0 / total).toFixed(2), '3MB/s');
    }, 100);
}

function cancleDownloadAndUpdate() {
    cancleFlag = true;
}