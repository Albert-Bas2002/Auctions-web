import type { NextConfig } from "next";

const nextConfig: NextConfig = {};
module.exports = {
  allowedDevOrigins: ["local-origin.dev", "*.local-origin.dev"],
  eslint: {
    ignoreDuringBuilds: true,
  },
};
export default nextConfig;
