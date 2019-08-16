package com.crashlytics.android.answers;

import android.content.Context;
import com.google.firebase.analytics.FirebaseAnalytics.Event;
import p017io.fabric.sdk.android.Fabric;

class FirebaseAnalyticsApiAdapter {
    private final Context context;
    private EventLogger eventLogger;
    private final FirebaseAnalyticsEventMapper eventMapper;

    public FirebaseAnalyticsApiAdapter(Context context2) {
        this(context2, new FirebaseAnalyticsEventMapper());
    }

    public FirebaseAnalyticsApiAdapter(Context context2, FirebaseAnalyticsEventMapper firebaseAnalyticsEventMapper) {
        this.context = context2;
        this.eventMapper = firebaseAnalyticsEventMapper;
    }

    public EventLogger getFirebaseAnalytics() {
        if (this.eventLogger == null) {
            this.eventLogger = AppMeasurementEventLogger.getEventLogger(this.context);
        }
        return this.eventLogger;
    }

    public void processEvent(SessionEvent sessionEvent) {
        EventLogger firebaseAnalytics = getFirebaseAnalytics();
        if (firebaseAnalytics == null) {
            Fabric.getLogger().mo20969d(Answers.TAG, "Firebase analytics logging was enabled, but not available...");
            return;
        }
        FirebaseAnalyticsEvent mapEvent = this.eventMapper.mapEvent(sessionEvent);
        if (mapEvent == null) {
            Fabric.getLogger().mo20969d(Answers.TAG, "Fabric event was not mappable to Firebase event: " + sessionEvent);
            return;
        }
        firebaseAnalytics.logEvent(mapEvent.getEventName(), mapEvent.getEventParams());
        if ("levelEnd".equals(sessionEvent.predefinedType)) {
            firebaseAnalytics.logEvent(Event.POST_SCORE, mapEvent.getEventParams());
        }
    }
}