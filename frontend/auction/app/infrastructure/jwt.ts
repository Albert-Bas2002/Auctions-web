import jwt from "jsonwebtoken";

export const getJwtUserId = () => {
  if (typeof document !== "undefined") {
    const cookies = document.cookie.split(";").map((cookie) => cookie.trim());
    const token = cookies
      .find((cookie) => cookie.startsWith("MyCookies="))
      ?.split("=")[1];

    if (token) {
      try {
        const decoded = jwt.decode(token) as { userId: string };
        return decoded?.userId || null;
      } catch (e) {
        return null;
      }
    }
  }
  return null;
};
