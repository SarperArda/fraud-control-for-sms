
---

# Fraud Control for SMS

Fraud Control for SMS is a project designed to detect fraudulent SMS messages using AI. It integrates with the Gemini AI model to analyze SMS content and provide fraud detection capabilities.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Running Tests](#running-tests)
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
   pip install google-cloud-generativai python-dotenv
   ```

2. **Create a `.env` File**

   In the root directory of your project, create a `.env` file and add your API key:

   ```env
   GEMINI_API_KEY=your_api_key_here
   ```

## Configuration

### Update Python Path in C#

Ensure the Python path in `Program.cs` is correct. Update the `FileName` property in the `ProcessStartInfo` to point to your Python executable. For example:

```csharp
start.FileName = "/usr/bin/python3"; // Update this path to your Python executable path
```

## Usage

To run the application, use the following command:

```bash
dotnet run
```

This will start the Fraud Control for SMS service. The application will analyze SMS content and detect potential fraud using the Gemini AI model.

### Example Usage

1. **Start the Application**

   Navigate to the project directory and run:

   ```bash
   dotnet run
   ```

2. **Send a Prompt**

   The application will prompt you to enter an SMS message for analysis. Enter the message and press Enter.

## Running Tests

To run the tests, use the following command:

```bash
dotnet test
```

This will execute all the unit tests in the project.

## Contributing

Contributions are welcome! If you would like to contribute to the project, please follow these steps:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes and commit them (`git commit -am 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a new Pull Request.

---
