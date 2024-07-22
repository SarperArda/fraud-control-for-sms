document.getElementById('uploadBtn').addEventListener('click', () => {
    document.getElementById('csvFileInput').click();
});

document.getElementById('csvFileInput').addEventListener('change', handleFileSelect);
document.getElementById('downloadBtn').addEventListener('click', downloadCSV);

let csvContent = '';

function handleFileSelect(event) {
    const file = event.target.files[0];
    if (file && file.type === 'text/csv') {
        const reader = new FileReader();
        reader.onload = function(e) {
            csvContent = e.target.result;
            document.getElementById('downloadBtn').disabled = false;
        };
        reader.readAsText(file);
    } else {
        alert('Please upload a valid CSV file.');
        document.getElementById('downloadBtn').disabled = true;
    }
}

function downloadCSV() {
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    link.setAttribute('href', url);
    link.setAttribute('download', 'downloaded.csv');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);  // Clean up the URL object
}
