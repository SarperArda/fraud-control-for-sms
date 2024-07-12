"""
    Description: This script is used to train a machine learning model for SMS spam detection.
    Author: Sarper Arda BAKIR
    Date: 09-07-2024
    Version: 1.0
"""

import pandas as pd
import tensorflow as tf
from tensorflow.keras.preprocessing.text import Tokenizer
from tensorflow.keras.preprocessing.sequence import pad_sequences
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Embedding, LSTM, Dense, Dropout
from sklearn.model_selection import train_test_split
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import classification_report

def train_sms_spam_model(input_message):
    # Step 1: Data Collection and Preprocessing
    # Load dataset from CSV file
    # Tiago Agostinho de Almeida and JosÈ MarÌa GÛmez Hidalgo hold the copyright (c) for the SMS Spam Collection v.1.
    # The dataset is available at https://archive.ics.uci.edu/dataset/228/sms+spam+collection 
    data = pd.read_csv('sms.csv', encoding='utf-8')
    data = data[['label', 'message']]

    # Encode labels
    label_encoder = LabelEncoder()
    labels = label_encoder.fit_transform(data['label'].values)
    
    # Text preprocessing
    texts = data['message'].values

    # Tokenization and padding
    tokenizer = Tokenizer(num_words=5000)
    tokenizer.fit_on_texts(texts)
    sequences = tokenizer.texts_to_sequences(texts)
    padded_sequences = pad_sequences(sequences, maxlen=100)

    # Split data into training and testing sets
    X_train, X_test, y_train, y_test = train_test_split(padded_sequences, labels, test_size=0.2, random_state=42)

    # Step 2: Model Building
    model = Sequential([
        Embedding(input_dim=5000, output_dim=64, input_length=100),
        LSTM(64, return_sequences=True),
        LSTM(32),
        Dropout(0.5),
        Dense(1, activation='sigmoid')
    ])

    model.compile(loss='binary_crossentropy', optimizer='adam', metrics=['accuracy'])

    # Step 3: Model Training
    model.fit(X_train, y_train, epochs=10, batch_size=64, validation_split=0.2)

    # Save the trained model
    model.save('sms_spam_model.h5')

    # Step 4: Model Evaluation
    y_pred = (model.predict(X_test) > 0.5).astype("int32")

    # Step 5: Example Prediction
    new_seq = tokenizer.texts_to_sequences([input_message])
    new_padded_seq = pad_sequences(new_seq, maxlen=100)
    prediction = model.predict(new_padded_seq)

    print("Spam Probability:", prediction[0][0])

if __name__ == "__main__":
    import sys
    if len(sys.argv) > 1:
        input_message = sys.argv[1]
        train_sms_spam_model(input_message)
    else:
        print("Please provide an input message for prediction.")
