// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
	const allSideMenu = document.querySelectorAll('#sidebar .side-menu.top li a');

	// Function to set the active menu item based on the current URL
	function setActiveMenu() {
		const currentUrl = window.location.href;

		allSideMenu.forEach(item => {
			const li = item.parentElement;
			if (item.href === currentUrl) {
				li.classList.add('active');
			} else {
				li.classList.remove('active');
			}
		});
	}

	// Set the active menu item on page load
	setActiveMenu();

	// Add click event listener to update active menu item
	allSideMenu.forEach(item => {
		const li = item.parentElement;

		item.addEventListener('click', function (event) {
			// Prevent default link behavior
			event.preventDefault();

			// Remove 'active' class from all menu items
			document.querySelectorAll('#sidebar .side-menu.top li').forEach(i => {
				i.classList.remove('active');
			});

			// Add 'active' class to the clicked menu item
			li.classList.add('active');

			// Optionally, navigate to the new page manually if needed
			window.location.href = this.href;
		});
	});
});






// TOGGLE SIDEBAR
const menuBar = document.querySelector('#content nav .bx.bx-menu');
const sidebar = document.getElementById('sidebar');

menuBar.addEventListener('click', function () {
	sidebar.classList.toggle('hide');
})

const switchMode = document.getElementById('switch-mode');

// Check and apply the saved theme on page load
document.addEventListener('DOMContentLoaded', () => {
	const isDarkMode = localStorage.getItem('darkMode') === 'true';
	if (isDarkMode) {
		document.body.classList.add('dark');
		switchMode.checked = true; // Ensure the switch reflects the state
	}
});

// Listen for changes and save the theme to localStorage
switchMode.addEventListener('change', function () {
	if (this.checked) {
		document.body.classList.add('dark');
		localStorage.setItem('darkMode', 'true');
	} else {
		document.body.classList.remove('dark');
		localStorage.setItem('darkMode', 'false');
	}
});
