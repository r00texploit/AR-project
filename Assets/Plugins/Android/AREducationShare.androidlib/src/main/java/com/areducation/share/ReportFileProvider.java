package com.areducation.share;

import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.MatrixCursor;
import android.net.Uri;
import android.os.ParcelFileDescriptor;
import android.provider.OpenableColumns;
import android.text.TextUtils;
import android.util.Base64;

import java.io.File;
import java.io.FileNotFoundException;
import java.nio.charset.StandardCharsets;
import java.util.List;

public final class ReportFileProvider extends ContentProvider {
    @Override
    public boolean onCreate() {
        return true;
    }

    @Override
    public String getType(Uri uri) {
        return "application/pdf";
    }

    @Override
    public Cursor query(Uri uri, String[] projection, String selection, String[] selectionArgs, String sortOrder) {
        try {
            File file = resolveSharedFile(uri);
            String[] columns = projection == null
                    ? new String[]{OpenableColumns.DISPLAY_NAME, OpenableColumns.SIZE}
                    : projection;
            Object[] values = new Object[columns.length];

            for (int i = 0; i < columns.length; i++) {
                if (OpenableColumns.DISPLAY_NAME.equals(columns[i])) {
                    values[i] = file.getName();
                } else if (OpenableColumns.SIZE.equals(columns[i])) {
                    values[i] = file.length();
                }
            }

            MatrixCursor cursor = new MatrixCursor(columns, 1);
            cursor.addRow(values);
            return cursor;
        } catch (Exception ignored) {
            return null;
        }
    }

    @Override
    public ParcelFileDescriptor openFile(Uri uri, String mode) throws FileNotFoundException {
        if (!TextUtils.isEmpty(mode) && mode.indexOf('w') >= 0) {
            throw new FileNotFoundException("Write access is not allowed.");
        }

        try {
            File file = resolveSharedFile(uri);
            return ParcelFileDescriptor.open(file, ParcelFileDescriptor.MODE_READ_ONLY);
        } catch (Exception ex) {
            throw new FileNotFoundException(ex.getMessage());
        }
    }

    @Override
    public Uri insert(Uri uri, ContentValues values) {
        throw new UnsupportedOperationException("Insert is not supported.");
    }

    @Override
    public int delete(Uri uri, String selection, String[] selectionArgs) {
        throw new UnsupportedOperationException("Delete is not supported.");
    }

    @Override
    public int update(Uri uri, ContentValues values, String selection, String[] selectionArgs) {
        throw new UnsupportedOperationException("Update is not supported.");
    }

    private File resolveSharedFile(Uri uri) throws Exception {
        List<String> segments = uri.getPathSegments();
        if (segments.size() != 2 || !"report".equals(segments.get(0))) {
            throw new FileNotFoundException("Unknown report URI.");
        }

        byte[] decoded = Base64.decode(segments.get(1), Base64.URL_SAFE | Base64.NO_WRAP);
        File file = new File(new String(decoded, StandardCharsets.UTF_8)).getCanonicalFile();
        Context context = getContext();
        if (context == null || !file.isFile() || !isAllowedPath(context, file)) {
            throw new FileNotFoundException("Report file is not available.");
        }

        return file;
    }

    private static boolean isAllowedPath(Context context, File file) throws Exception {
        return isInside(file, context.getFilesDir())
                || isInside(file, context.getCacheDir())
                || isInside(file, context.getExternalFilesDir(null))
                || isInside(file, context.getExternalCacheDir());
    }

    private static boolean isInside(File file, File root) throws Exception {
        if (root == null) {
            return false;
        }

        String filePath = file.getCanonicalPath();
        String rootPath = root.getCanonicalPath();
        return filePath.equals(rootPath) || filePath.startsWith(rootPath + File.separator);
    }
}
