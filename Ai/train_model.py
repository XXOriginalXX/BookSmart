import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.linear_model import LogisticRegression
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import classification_report, confusion_matrix, roc_auc_score
import pickle
import os

df = pd.read_csv("noshowappointments.csv")

df.rename(columns={
    "Hipertension": "Hypertension",
    "Handcap": "Handicap",
    "No-show": "NoShow"
}, inplace=True)

df = df[(df["Age"] >= 0) & (df["Age"] <= 97)]
df["Handicap"] = df["Handicap"].apply(lambda x: 1 if x > 0 else 0)

df["ScheduledDay"] = pd.to_datetime(df["ScheduledDay"])
df["AppointmentDay"] = pd.to_datetime(df["AppointmentDay"])

df["WaitingDays"] = (df["AppointmentDay"] - df["ScheduledDay"]).dt.days
df["WaitingDays"] = df["WaitingDays"].apply(lambda x: x if x >= 0 else 0)

df["AppointmentDayOfWeek"] = df["AppointmentDay"].dt.dayofweek
df["AppointmentHour"] = df["ScheduledDay"].dt.hour

df["NoShow"] = df["NoShow"].map({"Yes": 1, "No": 0})

le = LabelEncoder()
df["Gender"] = le.fit_transform(df["Gender"])
df["Neighbourhood"] = le.fit_transform(df["Neighbourhood"])

features = [
    "Gender", "Age", "Neighbourhood", "Scholarship",
    "Hypertension", "Diabetes", "Alcoholism", "Handicap",
    "SMS_received", "WaitingDays", "AppointmentDayOfWeek", "AppointmentHour"
]

X = df[features]
y = df["NoShow"]

X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)

THRESHOLD = 0.3

print("Training Random Forest...")
rf = RandomForestClassifier(n_estimators=100, class_weight="balanced", random_state=42)
rf.fit(X_train, y_train)

probs = rf.predict_proba(X_test)[:, 1]
predictions = (probs >= THRESHOLD).astype(int)

auc = roc_auc_score(y_test, probs)

print(f"\nAUC Score     : {round(auc, 4)}")
print(f"Threshold used: {THRESHOLD}")
print("\nClassification Report:")
print(classification_report(y_test, predictions, target_names=["Show Up", "No Show"]))

cm = confusion_matrix(y_test, predictions)
print("Confusion Matrix:")
print(cm)

tn, fp, fn, tp = cm.ravel()
print(f"\nTrue Positives  (No-shows caught)  : {tp}")
print(f"False Negatives (No-shows missed)  : {fn}")
print(f"False Positives (Wrong flags)      : {fp}")
print(f"True Negatives  (Correct show-ups) : {tn}")

print("\nTop Features by Importance:")
importance = pd.Series(rf.feature_importances_, index=features).sort_values(ascending=False)
for feat, score in importance.items():
    print(f"  {feat:<25} {round(score, 4)}")

os.makedirs("model", exist_ok=True)

with open("model/noshow_model.pkl", "wb") as f:
    pickle.dump(rf, f)

with open("model/feature_names.pkl", "wb") as f:
    pickle.dump(features, f)

with open("model/threshold.pkl", "wb") as f:
    pickle.dump(THRESHOLD, f)

print("\nDone. Model saved to model/")