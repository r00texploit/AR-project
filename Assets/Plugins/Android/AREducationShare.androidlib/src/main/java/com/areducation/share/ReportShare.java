package com.areducation.share;

import android.content.Context;
import android.net.Uri;
import android.util.Base64;

import java.io.File;
import java.nio.charset.StandardCharsets;

public final class ReportShare {
    private ReportShare() {
    }

    public static Uri getUriForFile(Context context, String authority, File file) throws Exception {
        String canonicalPath = file.getCanonicalPath();
        String token = Base64.encodeToString(
                canonicalPath.getBytes(StandardCharsets.UTF_8),
                Base64.URL_SAFE | Base64.NO_WRAP);

        return new Uri.Builder()
                .scheme("content")
                .authority(authority)
                .appendPath("report")
                .appendPath(token)
                .build();
    }
}
