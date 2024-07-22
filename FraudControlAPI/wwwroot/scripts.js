document.getElementById('uploadBtn').addEventListener('click', () => {
    document.getElementById('csvFileInput').click();
});

document.getElementById('csvFileInput').addEventListener('change', handleFileSelect);

let uploadedFile = null;

function handleFileSelect(event) {
    uploadedFile = event.target.files[0];
    if (uploadedFile) {
        uploadFile(uploadedFile);
    } else {
        alert('Please upload a valid file.');
        uploadedFile = null;
    }
}

function uploadFile(file) {
    const formData = new FormData();
    formData.append('file', file);  // 'file' must match the parameter name in the controller

    fetch('http://localhost:5163/api/fraudcontrol/analyze', {
        method: 'POST',
        body: formData
    })
    .then(response => {
        if (!response.ok) {
            throw new Error('Network response was not ok.');
        }
        return response.text(); // Assuming the response is text (CSV or JSON)
    })
    .then(data => {
        // Display the results
        console.log('Response:', data);
        document.getElementById('result').textContent = data;  // Display the result in a <div id="result"></div>
    })
    .catch(error => {
        console.error('Error:', error);
    });
}
