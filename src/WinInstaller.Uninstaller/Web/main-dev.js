function GetInstallLocation() {
    return "c:\\windows";
}

function UnInstall() {
    if(confirm('卸载前请注意备份文件,确定卸载?')){
        var total = 100;
        var handled = 0;
        var timer = setInterval(function () {
            if (handled >= total) {
                clearInterval(timer);
                setRunningState(false);
                setSuccessState(true);
                return;
            }
    
            setLog(`正在删除文件:${handled}.txt`);
            handled += 1;
        }, 100);
    }else{
        setRunningState(false);
    }
}


function Close(){
    alert('see you again');
}