document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("clearFilters").addEventListener("click", function () {
        let form = document.querySelector("form");
        form.reset();
        window.location.href = "/Project/Index"; // Adjust if your controller name is different
    });
});
