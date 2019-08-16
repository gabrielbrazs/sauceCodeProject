package com.google.android.gms.internal.measurement;

import java.io.Serializable;
import org.checkerframework.checker.nullness.compatqual.NullableDecl;

final class zzdd<T> implements zzdb<T>, Serializable {
    @NullableDecl
    private transient T value;
    private final zzdb<T> zzabs;
    private volatile transient boolean zzdh;

    zzdd(zzdb<T> zzdb) {
        this.zzabs = (zzdb) zzcz.checkNotNull(zzdb);
    }

    public final T get() {
        if (!this.zzdh) {
            synchronized (this) {
                if (!this.zzdh) {
                    T t = this.zzabs.get();
                    this.value = t;
                    this.zzdh = true;
                    return t;
                }
            }
        }
        return this.value;
    }

    public final String toString() {
        Object obj;
        if (this.zzdh) {
            String valueOf = String.valueOf(this.value);
            obj = new StringBuilder(String.valueOf(valueOf).length() + 25).append("<supplier that returned ").append(valueOf).append(">").toString();
        } else {
            obj = this.zzabs;
        }
        String valueOf2 = String.valueOf(obj);
        return new StringBuilder(String.valueOf(valueOf2).length() + 19).append("Suppliers.memoize(").append(valueOf2).append(")").toString();
    }
}