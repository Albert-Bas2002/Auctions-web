"use client";
import React from "react";
import { Layout, Row, Col, Space } from "antd";
import { GithubOutlined, LinkedinOutlined } from "@ant-design/icons";

const AppFooter: React.FC = () => {
  return (
    <Layout.Footer style={{ backgroundColor: "#f0f2f5", padding: "24px 50px" }}>
      <Row justify="space-between" align="middle">
        <Col>
          <Space direction="vertical">
            <div>
              © {new Date().getFullYear()} MyCompany. All rights reserved.
            </div>
            <div>
              <a href="/privacy" style={{ marginRight: 16 }}>
                Privacy Policy
              </a>
              <a href="/terms">Terms of Service</a>
            </div>
          </Space>
        </Col>
        <Col>
          <Space size="middle">
            <a
              href="https://github.com/"
              target="_blank"
              rel="noopener noreferrer"
            >
              <GithubOutlined style={{ fontSize: "18px" }} />
            </a>
            <a
              href="https://linkedin.com/"
              target="_blank"
              rel="noopener noreferrer"
            >
              <LinkedinOutlined style={{ fontSize: "18px" }} />
            </a>
          </Space>
        </Col>
      </Row>
    </Layout.Footer>
  );
};

export default AppFooter;
