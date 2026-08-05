import { apiOrigin } from "../api/client";

// Product images may be an absolute external URL (e.g. seed/demo data) or a
// server-relative path returned by the upload endpoint (e.g. "/uploads/products/...").
export function resolveImageUrl(imageUrl: string): string {
  return imageUrl.startsWith("/") ? `${apiOrigin}${imageUrl}` : imageUrl;
}
