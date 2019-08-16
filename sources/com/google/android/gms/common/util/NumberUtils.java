package com.google.android.gms.common.util;

import com.google.android.gms.common.annotation.KeepForSdk;

@KeepForSdk
@VisibleForTesting
public class NumberUtils {
    private NumberUtils() {
    }

    @KeepForSdk
    public static long parseHexLong(String str) {
        if (str.length() <= 16) {
            return str.length() == 16 ? Long.parseLong(str.substring(8), 16) | (Long.parseLong(str.substring(0, 8), 16) << 32) : Long.parseLong(str, 16);
        }
        throw new NumberFormatException(new StringBuilder(String.valueOf(str).length() + 37).append("Invalid input: ").append(str).append(" exceeds 16 characters").toString());
    }
}