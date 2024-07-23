# Use the .NET 8.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory in the container
WORKDIR /app

# Copy the .csproj and restore the dependencies
COPY FraudControlAPI.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . .

# Build the .NET application
RUN dotnet build -c Release -o /app/build

# Publish the .NET application
RUN dotnet publish -c Release -o /app/publish

# Use the official Python image for running Python scripts
FROM python:3.9-slim AS python

# Set the working directory in the container
WORKDIR /app

# Install required Python packages
COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

# Copy Python scripts and model files
COPY Services/Tensorflow/fraud_detection_by_ml.py ./
COPY Services/OpenAI/openai_script.py ./
COPY Services/IPQS/url_scanner_api.py ./
COPY Services/IPQS/ip_reputation_api.py ./
COPY Services/Gemini/gemini_api.py ./
COPY sms_spam_model.h5 ./
COPY tokenizer.pickle ./

# Set the working directory
WORKDIR /app

# Copy the .csproj and restore the dependencies
COPY FraudControlAPI.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish

# Use the .NET 8.0 runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Expose port 5163
EXPOSE 5163

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "FraudControlAPI.dll"]
