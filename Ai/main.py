import pickle
import numpy as np
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

app = FastAPI()

with open("model/noshow_model.pkl", "rb") as f:
    model = pickle.load(f)

with open("model/feature_names.pkl", "rb") as f:
    feature_names = pickle.load(f)

with open("model/threshold.pkl", "rb") as f:
    threshold = pickle.load(f)


class AppointmentData(BaseModel):
    gender: int
    age: int
    neighbourhood: int
    scholarship: int
    hypertension: int
    diabetes: int
    alcoholism: int
    handicap: int
    sms_received: int
    waiting_days: int
    appointment_day_of_week: int
    appointment_hour: int


@app.post("/predict")
def predict(data: AppointmentData):
    try:
        row = np.array([[
            data.gender,
            data.age,
            data.neighbourhood,
            data.scholarship,
            data.hypertension,
            data.diabetes,
            data.alcoholism,
            data.handicap,
            data.sms_received,
            data.waiting_days,
            data.appointment_day_of_week,
            data.appointment_hour
        ]])

        prob = model.predict_proba(row)[0][1]
        will_no_show = bool(prob >= threshold)

        return {
            "noShowProbability": round(float(prob), 4),
            "willNoShow": will_no_show,
            "threshold": threshold
        }

    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@app.get("/health")
def health():
    return {"status": "ok"}