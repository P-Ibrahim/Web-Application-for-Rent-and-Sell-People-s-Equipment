var x = document.querySelector.getElementById('selling');
var y = document.querySelector.getElementById('rental');
var z = document.querySelector.getElementById('btn');
window.onload = function rental(){
    // x.style.left = "-400px";
    // y.style.left = "50px";
    x.style.display="none";
    y.style.display="block";
    z.style.left = "150px";
}
window.onload = function selling(){
    // x.style.left = "50px";
    // y.style.left = "450px";
    x.style.display="block";
    y.style.display="none";
    z.style.left = "0";
}