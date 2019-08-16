package net.gogame.gopay.vip.tape2;

import java.io.File;
import java.io.IOException;
import java.util.ConcurrentModificationException;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.NoSuchElementException;

/* renamed from: net.gogame.gopay.vip.tape2.b */
final class C1672b<T> extends ObjectQueue<T> {

    /* renamed from: a */
    final LinkedList<T> f1398a = new LinkedList<>();

    /* renamed from: b */
    int f1399b = 0;
    /* access modifiers changed from: private */

    /* renamed from: c */
    public boolean f1400c;

    /* renamed from: net.gogame.gopay.vip.tape2.b$a */
    private final class C1673a implements Iterator<T> {

        /* renamed from: a */
        int f1401a = 0;

        /* renamed from: b */
        int f1402b = C1672b.this.f1399b;

        C1673a() {
        }

        public boolean hasNext() {
            m1009a();
            return this.f1401a != C1672b.this.size();
        }

        public T next() {
            if (C1672b.this.f1400c) {
                throw new IllegalStateException("closed");
            }
            m1009a();
            if (this.f1401a >= C1672b.this.size()) {
                throw new NoSuchElementException();
            }
            LinkedList<T> linkedList = C1672b.this.f1398a;
            int i = this.f1401a;
            this.f1401a = i + 1;
            return linkedList.get(i);
        }

        public void remove() {
            if (C1672b.this.f1400c) {
                throw new IllegalStateException("closed");
            }
            m1009a();
            if (C1672b.this.size() == 0) {
                throw new NoSuchElementException();
            } else if (this.f1401a != 1) {
                throw new UnsupportedOperationException("Removal is only permitted from the head.");
            } else {
                try {
                    C1672b.this.remove();
                    this.f1402b = C1672b.this.f1399b;
                    this.f1401a--;
                } catch (IOException e) {
                    throw new RuntimeException("todo: throw a proper error", e);
                }
            }
        }

        /* renamed from: a */
        private void m1009a() {
            if (C1672b.this.f1399b != this.f1402b) {
                throw new ConcurrentModificationException();
            }
        }
    }

    C1672b() {
    }

    public File file() {
        return null;
    }

    public void add(T t) throws IOException {
        if (this.f1400c) {
            throw new IOException("closed");
        }
        this.f1399b++;
        this.f1398a.add(t);
    }

    public T peek() throws IOException {
        if (!this.f1400c) {
            return this.f1398a.peek();
        }
        throw new IOException("closed");
    }

    public int size() {
        return this.f1398a.size();
    }

    public void remove(int i) throws IOException {
        if (this.f1400c) {
            throw new IOException("closed");
        }
        this.f1399b++;
        for (int i2 = 0; i2 < i; i2++) {
            this.f1398a.remove();
        }
    }

    public Iterator<T> iterator() {
        return new C1673a();
    }

    public void close() throws IOException {
        this.f1400c = true;
    }
}