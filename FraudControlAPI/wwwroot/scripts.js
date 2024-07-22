document.getElementById('uploadBtn').addEventListener('click', () => {
    document.getElementById('csvFileInput').click();
});

document.getElementById('csvFileInput').addEventListener('change', handleFileSelect);

let uploadedFile = null;

function handleFileSelect(event) {
    uploadedFile = event.target.files[0];
    if (uploadedFile && uploadedFile.type === 'text/csv') {
        uploadFile(uploadedFile);
    } else {
        alert('Please upload a valid CSV file.');
        uploadedFile = null;
    }
}

function uploadFile(file) {
    const formData = new FormData();
    formData.append('file', file);
    fetch('http://localhost:5163/api/fraudcontrol/analyze', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok.');
        }
        return response.blob(); // Get the response as a Blob
    })
    .then(blob => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = 'results.csv'; // Name the file to download
        link.click(); // Trigger the download
        URL.revokeObjectURL(url); // Clean up the URL object
    })
    .catch(error => {
        console.error('Error:', error);
    });
}
