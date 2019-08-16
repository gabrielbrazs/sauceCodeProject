package com.facebook.appevents.internal;

import android.app.Activity;
import android.app.Application;
import android.app.Application.ActivityLifecycleCallbacks;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import com.facebook.FacebookSdk;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import java.util.Map.Entry;
import java.util.concurrent.atomic.AtomicBoolean;
import org.json.JSONException;
import org.json.JSONObject;
import org.onepf.oms.appstore.AmazonAppstoreBillingService;
import org.onepf.oms.appstore.GooglePlay;

public class InAppPurchaseActivityLifecycleTracker {
    private static final String BILLING_ACTIVITY_NAME = "com.android.billingclient.api.ProxyBillingActivity";
    private static final String SERVICE_INTERFACE_NAME = "com.android.vending.billing.IInAppBillingService$Stub";
    private static final String TAG = InAppPurchaseActivityLifecycleTracker.class.getCanonicalName();
    private static ActivityLifecycleCallbacks callbacks;
    /* access modifiers changed from: private */
    public static Boolean hasBiillingActivity = null;
    private static Boolean hasBillingService = null;
    /* access modifiers changed from: private */
    public static Object inAppBillingObj;
    private static Intent intent;
    private static final AtomicBoolean isTracking = new AtomicBoolean(false);
    private static ServiceConnection serviceConnection;

    private static void initializeIfNotInitialized() {
        if (hasBillingService == null) {
            try {
                Class.forName(SERVICE_INTERFACE_NAME);
                hasBillingService = Boolean.valueOf(true);
                try {
                    Class.forName(BILLING_ACTIVITY_NAME);
                    hasBiillingActivity = Boolean.valueOf(true);
                } catch (ClassNotFoundException e) {
                    hasBiillingActivity = Boolean.valueOf(false);
                }
                InAppPurchaseEventManager.clearSkuDetailsCache();
                intent = new Intent(GooglePlay.VENDING_ACTION).setPackage("com.android.vending");
                serviceConnection = new ServiceConnection() {
                    public void onServiceConnected(ComponentName componentName, IBinder iBinder) {
                        InAppPurchaseActivityLifecycleTracker.inAppBillingObj = InAppPurchaseEventManager.asInterface(FacebookSdk.getApplicationContext(), iBinder);
                    }

                    public void onServiceDisconnected(ComponentName componentName) {
                    }
                };
                callbacks = new ActivityLifecycleCallbacks() {
                    public void onActivityCreated(Activity activity, Bundle bundle) {
                    }

                    public void onActivityDestroyed(Activity activity) {
                    }

                    public void onActivityPaused(Activity activity) {
                    }

                    public void onActivityResumed(Activity activity) {
                        FacebookSdk.getExecutor().execute(new Runnable() {
                            public void run() {
                                Context applicationContext = FacebookSdk.getApplicationContext();
                                InAppPurchaseActivityLifecycleTracker.logPurchaseInapp(applicationContext, InAppPurchaseEventManager.getPurchasesInapp(applicationContext, InAppPurchaseActivityLifecycleTracker.inAppBillingObj));
                                Map purchasesSubs = InAppPurchaseEventManager.getPurchasesSubs(applicationContext, InAppPurchaseActivityLifecycleTracker.inAppBillingObj);
                                Iterator it = InAppPurchaseEventManager.getPurchasesSubsExpire(applicationContext, InAppPurchaseActivityLifecycleTracker.inAppBillingObj).iterator();
                                while (it.hasNext()) {
                                    purchasesSubs.put((String) it.next(), SubscriptionType.EXPIRE);
                                }
                                InAppPurchaseActivityLifecycleTracker.logPurchaseSubs(applicationContext, purchasesSubs);
                            }
                        });
                    }

                    public void onActivitySaveInstanceState(Activity activity, Bundle bundle) {
                    }

                    public void onActivityStarted(Activity activity) {
                    }

                    public void onActivityStopped(Activity activity) {
                        if (InAppPurchaseActivityLifecycleTracker.hasBiillingActivity.booleanValue() && activity.getLocalClassName().equals(InAppPurchaseActivityLifecycleTracker.BILLING_ACTIVITY_NAME)) {
                            FacebookSdk.getExecutor().execute(new Runnable() {
                                public void run() {
                                    Context applicationContext = FacebookSdk.getApplicationContext();
                                    ArrayList purchasesInapp = InAppPurchaseEventManager.getPurchasesInapp(applicationContext, InAppPurchaseActivityLifecycleTracker.inAppBillingObj);
                                    if (purchasesInapp.isEmpty()) {
                                        purchasesInapp = InAppPurchaseEventManager.getPurchaseHistoryInapp(applicationContext, InAppPurchaseActivityLifecycleTracker.inAppBillingObj);
                                    }
                                    InAppPurchaseActivityLifecycleTracker.logPurchaseInapp(applicationContext, purchasesInapp);
                                }
                            });
                        }
                    }
                };
            } catch (ClassNotFoundException e2) {
                hasBillingService = Boolean.valueOf(false);
            }
        }
    }

    /* access modifiers changed from: private */
    public static void logPurchaseInapp(Context context, ArrayList<String> arrayList) {
        if (!arrayList.isEmpty()) {
            HashMap hashMap = new HashMap();
            ArrayList arrayList2 = new ArrayList();
            Iterator it = arrayList.iterator();
            while (it.hasNext()) {
                String str = (String) it.next();
                try {
                    String string = new JSONObject(str).getString(AmazonAppstoreBillingService.JSON_KEY_PRODUCT_ID);
                    hashMap.put(string, str);
                    arrayList2.add(string);
                } catch (JSONException e) {
                    Log.e(TAG, "Error parsing in-app purchase data.", e);
                }
            }
            for (Entry entry : InAppPurchaseEventManager.getSkuDetails(context, arrayList2, inAppBillingObj, false).entrySet()) {
                AutomaticAnalyticsLogger.logPurchaseInapp((String) hashMap.get(entry.getKey()), (String) entry.getValue());
            }
        }
    }

    /* access modifiers changed from: private */
    public static void logPurchaseSubs(Context context, Map<String, SubscriptionType> map) {
        if (!map.isEmpty()) {
            HashMap hashMap = new HashMap();
            ArrayList arrayList = new ArrayList();
            for (String str : map.keySet()) {
                try {
                    String string = new JSONObject(str).getString(AmazonAppstoreBillingService.JSON_KEY_PRODUCT_ID);
                    arrayList.add(string);
                    hashMap.put(string, str);
                } catch (JSONException e) {
                    Log.e(TAG, "Error parsing in-app purchase data.", e);
                }
            }
            Map skuDetails = InAppPurchaseEventManager.getSkuDetails(context, arrayList, inAppBillingObj, true);
            for (String str2 : skuDetails.keySet()) {
                String str3 = (String) hashMap.get(str2);
                AutomaticAnalyticsLogger.logPurchaseSubs((SubscriptionType) map.get(str3), str3, (String) skuDetails.get(str2));
            }
        }
    }

    private static void startTracking() {
        if (isTracking.compareAndSet(false, true)) {
            Context applicationContext = FacebookSdk.getApplicationContext();
            if (applicationContext instanceof Application) {
                ((Application) applicationContext).registerActivityLifecycleCallbacks(callbacks);
                applicationContext.bindService(intent, serviceConnection, 1);
            }
        }
    }

    public static void update() {
        initializeIfNotInitialized();
        if (hasBillingService.booleanValue() && AutomaticAnalyticsLogger.isImplicitPurchaseLoggingEnabled()) {
            startTracking();
        }
    }
}