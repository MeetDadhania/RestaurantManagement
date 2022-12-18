var toasts = [
    { title: 'Warning!', content: 'There was a problem with your network connection.', cssClass: 'e-toast-warning', icon: 'e-warning toast-icons' },
    { title: 'Success!', content: 'User Register successfully.', cssClass: 'e-toast-success', icon: 'e-success toast-icons' },
    { title: 'Error!', content: 'A problem has been occurred while submitting your data.', cssClass: 'e-toast-danger', icon: 'e-error toast-icons' },
    { title: 'Information!', content: 'Please read the comments carefully.', cssClass: 'e-toast-info', icon: 'e-info toast-icons' }];
var btnEleHide = document.getElementById('hideToast');
var toastObj;
setTimeout(function () {
    toastObj.target = document.body;
    toastObj.show(toasts[3]);
}, 500);
var infoBtn = document.getElementById('info_Toast');
var warnBtn = document.getElementById('warning_Toast');
var successBtn = document.getElementById('success_Toast');
var errorBtn = document.getElementById('error_Toast');
document.onclick = function (e) {
    var toastObj = document.getElementById('toast_type').ej2_instances[0];
    if (e.target !== infoBtn && e.target !== warnBtn && e.target !== successBtn && e.target !== errorBtn) {
        toastObj.hide('All');
    }
};

document.getElementById('hideToast').onclick = function () {
    toastObj.hide('All');
};
infoBtn.onclick = function () {
    toastObj.show(toasts[3]);
};
warnBtn.onclick = function () {
    toastObj.show(toasts[0]);
};
successBtn.onclick = function () {
    toastObj.show(toasts[1]);
};
errorBtn.onclick = function () {
    toastObj.show(toasts[2]);
};
function created() {
    toastObj = this;
}
function onclose(e) {
    if (e.toastContainer.childElementCount === 0) {
        btnEleHide.style.display = 'none';
    }
}

function onBeforeOpen() {
    btnEleHide.style.display = 'inline-block';
}