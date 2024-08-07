# Fraud Control Project

## Overview

The Fraud Control Project is a comprehensive solution for analyzing and scoring SMS messages for potential fraud or spam risks. It consists of two primary components:

### 1. Fraud Control API

The Fraud Control API is a web service built using .NET 8.0, designed to provide a robust fraud detection mechanism by integrating various AI models and external services.

#### Services

- **GeminiAI**: This service uses a machine learning model to evaluate SMS messages and provide a risk score. The score indicates the likelihood of the message being fraudulent or spammy.
- **TensorFlow Model**: This model processes SMS messages with TensorFlow to generate a risk score based on its trained machine learning model. The score is standardized to a 0 to 100 range.
- **IPQS**: This service checks any URLs present in the SMS message against a database of known malicious IP addresses and URLs. It provides a score or -1 if no URL is found.
- **OpenAI**: This service uses advanced language models from OpenAI to analyze the content of SMS messages and determine if they are likely to be fraudulent or spammy.

#### Scoring System

The final fraud score is a weighted combination of scores from the above services:
- **GeminiAI Score**: 30%
- **TensorFlow Score**: 20%
- **IPQS Score**: 10% (only if a URL is present, otherwise -1)
- **OpenAI Score**: 40%

The final score is calculated as:
- **With IPQS Score**: `Final Score = (GeminiAI Score * 0.3) + (TensorFlow Score * 0.2) + (IPQS Score * 0.1) + (OpenAI Score * 0.4)`
- **Without IPQS Score**: `Final Score = (GeminiAI Score * 0.3) + (TensorFlow Score * 0.3) + (OpenAI Score * 0.5)`

**Score Ranges and Risk Levels:**
- **High Risk**: Final Score >= 80
- **Moderate Risk**: 50 <= Final Score < 80
- **Low Risk**: Final Score < 50

### 2. Fraud Control for SMS

Fraud Control for SMS is a standalone tool designed to detect fraudulent SMS messages using various AI models and external APIs.

#### Detailed Explanation of the Program

- **OpenAI Model**: This model analyzes SMS content using OpenAI's language models to detect any suspicious patterns and provides a fraud probability score.
- **Gemini AI Model**: Similar to OpenAI, this model evaluates SMS messages for potential fraud using a machine learning model and provides a risk score.
- **TensorFlow Model**: This model uses TensorFlow to determine if an SMS is spam, based on its trained dataset, and provides a spam probability score.
- **IPQS API**: This API assesses URLs included in SMS messages for potential threats such as phishing or malware and provides a fraud probability score.

#### Combined Fraud Detection

The fraud detection program aggregates the scores from the Gemini AI model, TensorFlow model, and IPQS API to generate a final risk score. This score is a weighted average of the individual scores, giving a comprehensive assessment of the SMS message’s risk level.

---
