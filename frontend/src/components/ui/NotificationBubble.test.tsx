import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { NotificationBubble } from "./NotificationBubble";

describe("NotificationBubble", () => {
  it("renders nothing when count is zero", () => {
    const { container } = render(<NotificationBubble count={0} />);
    expect(container).toBeEmptyDOMElement();
  });

  it("renders nothing when count is negative", () => {
    const { container } = render(<NotificationBubble count={-1} />);
    expect(container).toBeEmptyDOMElement();
  });

  it("shows the exact count when 9 or below", () => {
    render(<NotificationBubble count={3} />);
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("caps the display at 9+ for larger counts", () => {
    render(<NotificationBubble count={42} />);
    expect(screen.getByText("9+")).toBeInTheDocument();
  });
});
