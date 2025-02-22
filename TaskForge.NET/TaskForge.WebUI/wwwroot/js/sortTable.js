// wwwroot/js/sortTable.js

function sortTable(columnName) {
    let currentUrl = new URL(window.location.href);
    let currentSortBy = currentUrl.searchParams.get('sortBy');
    let currentSortOrder = currentUrl.searchParams.get('sortOrder') === 'asc' ? 'desc' : 'asc';

    // If a new column is clicked, reset sortOrder to ascending
    if (currentSortBy !== columnName) {
        currentSortOrder = 'asc';
    }

    currentUrl.searchParams.set('sortBy', columnName);
    currentUrl.searchParams.set('sortOrder', currentSortOrder);
    window.location.href = currentUrl.toString();
}
