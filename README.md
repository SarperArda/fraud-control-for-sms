# Fraud Control for SMS

Fraud Control for SMS is a project designed to detect fraudulent SMS messages using AI. It integrates with the Gemini AI model, TensorFlow model, and IPQS API to analyze SMS content and provide fraud detection capabilities.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Contributing](#contributing)

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or later)
- [Python](https://www.python.org/downloads/) (version 3.11 or later)
- [Python packages](#python-packages)

## Installation

### Clone the Repository

Clone this repository to your local machine:

```bash
git clone https://github.com/your-username/fraud-control-for-sms.git
cd fraud-control-for-sms
```

### Install .NET Dependencies

Navigate to the project directory and restore the .NET dependencies:

```bash
cd FraudControlForSMS
dotnet restore
```

### Set Up Python Environment

1. **Install Python Packages**

   Ensure you have Python and pip installed. Then, install the required Python packages:

   ```bash
   pip install google-cloud-generativai python-dotenv requests pandas tensorflow
   ```

## Configuration

### Create a `.env` File

   In the root directory of your project, create a `.env` file. Obtain API keys individually from https://ai.google.dev and https://www.ipqualityscore.com/documentation/overview:

   ```env
   GEMINI_API_KEY=your_gemini_api_key_here
   IP_QUALITY_SCORE_API_KEY=your_ip_quality_score_api_key_here
   PYTHON_INTERPRETER=path_to_your_python_executable
   GEMINI_SCRIPT=path_to_your_gemini_script
   TENSORFLOW_SCRIPT=path_to_your_tensorflow_script
   IPQS_SCRIPT=path_to_your_ipqs_script
   ```

## Usage

To run the application, use the following command:

```bash
dotnet run
```

This will start the Fraud Control for SMS service. The application will analyze SMS content and detect potential fraud using the Gemini AI model, TensorFlow model, and IPQS API.

### Example Usage

1. **Start the Application**

   Navigate to the project directory and run:

   ```bash
   dotnet run
   ```

2. **Send a Prompt**

   The application will analyze a predefined SMS message for fraud risk by utilizing the Gemini AI model, TensorFlow model, and IPQS API. The results will provide a comprehensive assessment of the message's risk level.

## Detailed Explanation of the Program

### Gemini AI Model

The Gemini AI model is a generative AI model that analyzes the content of SMS messages to detect any suspicious or spam-like patterns. It provides a fraud probability score based on the input message content.

### TensorFlow Model

The TensorFlow model is a machine learning model trained on a dataset of SMS messages. It evaluates the likelihood of the message being spam based on its content and provides a spam probability score.

### IPQS API

The IPQS (IP Quality Score) API assesses URLs included in the SMS message for potential phishing, malware, or other fraudulent activities. It provides a fraud probability score based on the URL analysis.

### Combined Fraud Detection

The program combines the scores from the Gemini AI model, TensorFlow model, and IPQS API to generate a final fraud risk score. The final score is a weighted average of the individual scores, providing a comprehensive analysis of the SMS message's risk level.

## Contributing

Contributions are welcome! If you would like to contribute to the project, please follow these steps:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes and commit them (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a new Pull Request.
