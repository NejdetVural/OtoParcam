import { describe, expect, it } from "vitest";
import { resolveImageUrl } from "./images";

describe("resolveImageUrl", () => {
  it("prefixes the API origin onto a server-relative upload URL", () => {
    expect(resolveImageUrl("/uploads/products/abc/1.png")).toBe(
      "http://localhost:5284/uploads/products/abc/1.png",
    );
  });

  it("passes an absolute external URL through unchanged", () => {
    expect(resolveImageUrl("https://placehold.co/400x300")).toBe("https://placehold.co/400x300");
  });

  it("passes an http external URL through unchanged", () => {
    expect(resolveImageUrl("http://example.com/photo.jpg")).toBe("http://example.com/photo.jpg");
  });
});
