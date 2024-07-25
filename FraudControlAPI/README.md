# Fraud Control API

## Overview

The Fraud Control API is a web service designed to analyze and score SMS messages for potential fraud or spam risks using multiple AI models and external services. The API integrates with various fraud detection services and models to provide a comprehensive risk assessment for each message.

## Features

- **Multi-Model Analysis**: Utilizes GeminiAI, TensorFlow, IPQS, and OpenAI to analyze SMS messages.
- **Fraud Scoring**: Calculates a final risk score based on the analysis from different models.
- **Risk Explanation**: Provides an explanation of the risk level based on the final score.

## Technologies Used

- **.NET 8.0**: The API is built on .NET 8.0 for robust performance and scalability.
- **Python Integration**: Utilizes Python scripts for AI model predictions.
- **CSV Handling**: Processes input and output using CSV files.

## Services

### 1. GeminiAI

**GeminiAI** is a service that evaluates SMS messages using a specific machine learning model designed for detecting fraud or spam. It returns a score indicating the likelihood that the message is fraudulent or spammy.

**How It Works:**
- Executes a Python script to analyze the SMS content.
- Returns a score between 0 and 100, where a higher score indicates higher risk.

### 2. TensorFlow Model

The **TensorFlow Model** is used to predict the risk level of SMS messages based on trained machine learning models.

**How It Works:**
- Uses TensorFlow to process the SMS message and generate a risk score.
- The score is multiplied by 100 and rounded to two decimal places to standardize it within the 0 to 100 range.

### 3. IPQS

**IPQS** checks the URLs within SMS messages against a database of known malicious IP addresses and URLs.

**How It Works:**
- Extracts URLs from the SMS content.
- Checks these URLs against the IPQS database to identify potential threats.
- Returns a score or -1 if no URL is found.

### 4. OpenAI

**OpenAI** uses advanced language models to analyze the content of SMS messages and determine if they are likely to be fraudulent or spammy.

**How It Works:**
- Executes a Python script to analyze the SMS content.
- Returns a score indicating the risk level, similar to the GeminiAI.

## Scoring System

The final fraud score is calculated by combining the scores from the different services, with weights assigned to each service based on its perceived importance:

1. **GeminiAI Score**: 30%
2. **TensorFlow Score**: 20%
3. **IPQS Score**: 10% (if a URL is present, otherwise -1)
4. **OpenAI Score**: 40%

**Score Calculation Formula:**

- If IPQS score is available:
  ```text
  Final Score = (GeminiAI Score * 0.3) + (TensorFlow Score * 0.2) + (IPQS Score * 0.1) + (OpenAI Score * 0.4)
  ```
- If IPQS score is not available:
  ```text
  Final Score = (GeminiAI Score * 0.3) + (TensorFlow Score * 0.3) + (OpenAI Score * 0.5)
  ```

**Score Ranges and Risk Levels:**

- **High Risk**: Final Score >= 80
- **Moderate Risk**: 50 <= Final Score < 80
- **Low Risk**: Final Score < 50

## Setup and Installation

### Prerequisites

- .NET 8.0 SDK
- Python 3.x
- Required Python libraries (listed in `requirements.txt`)
- Environment variables for configuration (see `.env` file)

### Clone the Repository

```bash
git clone https://github.com/yourusername/fraud-control-api.git
cd fraud-control-api
```

### Install .NET Dependencies

```bash
dotnet restore
```

### Set Up Python Environment

1. Create a Python virtual environment:
   ```bash
   python -m venv venv
   ```
2. Activate the virtual environment:
   - On Windows: `venv\Scripts\activate`
   - On macOS/Linux: `source venv/bin/activate`
3. Install Python dependencies:
   ```bash
   pip install -r requirements.txt
   ```

### Configuration

- Copy the `.env.example` file to `.env` and update the environment variables as needed:
  ```bash
  cp .env.example .env
  ```

### Running the Application

1. Start the API server:
   ```bash
   dotnet run
   ```
2. The server will be available at `https://localhost:5001`.

## Usage

### Analyzing SMS Messages

Send a POST request to `/api/fraudcontrol` with a JSON body containing an array of SMS messages:

**Endpoint:** `POST /api/fraudcontrol`

**Request Body Example:**
```json
[
    { "Message": "Congratulations! You've won a $1000 gift card. Click here to claim now!" },
    { "Message": "Hi, just checking in. Hope you are doing well!" }
]
```

**Response Example:**
```json
[
    {
        "Message": "Congratulations! You've won a $1000 gift card. Click here to claim now!",
        "GeminiScore": 75.5,
        "TensorFlowScore": 80.0,
        "IPQSScore": 90.0,
        "OpenAIScore": 70.0,
        "FinalScore": 80,
        "Explanation": "The final fraud/spam risk score is 80, which indicates a high risk level."
    },
    {
        "Message": "Hi, just checking in. Hope you are doing well!",
        "GeminiScore": 20.0,
        "TensorFlowScore": 10.0,
        "IPQSScore": -1,
        "OpenAIScore": 15.0,
        "FinalScore": 16,
        "Explanation": "The final fraud/spam risk score is 16, which indicates a low risk level."
    }
]
```

## Contributing

Feel free to submit issues or pull requests. Ensure to follow coding standards and write tests for new features or bug fixes.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---
