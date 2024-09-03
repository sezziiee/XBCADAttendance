// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ScanUserID() {
    fetch("https://localhost:7037/api/Attendance/Scan")
        .then((response) => response.text())
        .then((userID) => {
            console.log(userID);
            document.getElementById("userIDInput").value = userID; // Update the button's value to the scanned user ID
        })
        .catch((error) => console.error("Error:", error));            
}