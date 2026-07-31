-- Create the database (run this in phpMyAdmin or MySQL CLI)
CREATE DATABASE IF NOT EXISTS `ads-tracking` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `ads-tracking`;

-- Visit records table
CREATE TABLE IF NOT EXISTS visit_records (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    visitor_id CHAR(36) NOT NULL,
    utm_source VARCHAR(256) NOT NULL,
    utm_medium VARCHAR(256) NOT NULL,
    utm_campaign VARCHAR(256) NOT NULL,
    utm_term VARCHAR(256) NOT NULL DEFAULT '',
    utm_content VARCHAR(256) NOT NULL DEFAULT '',
    fbclid VARCHAR(512) NOT NULL DEFAULT '',
    timestamp_utc DATETIME(3) NOT NULL,
    ip_address VARCHAR(45) NOT NULL,
    user_agent VARCHAR(1024) NOT NULL,
    created_at DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),

    UNIQUE KEY uq_visitor_timestamp (visitor_id, timestamp_utc),
    INDEX idx_visitor_id (visitor_id),
    INDEX idx_utm_source_medium_campaign (utm_source, utm_medium, utm_campaign),
    INDEX idx_timestamp_utc (timestamp_utc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Telegram click events table
CREATE TABLE IF NOT EXISTS telegram_click_events (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    visitor_id CHAR(36) NOT NULL,
    timestamp_utc DATETIME(3) NOT NULL,
    created_at DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),

    INDEX idx_tg_visitor_id (visitor_id),
    INDEX idx_tg_timestamp_utc (timestamp_utc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Download events table
CREATE TABLE IF NOT EXISTS download_events (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    timestamp_utc DATETIME(3) NOT NULL,
    ip_address VARCHAR(45) NOT NULL,
    user_agent VARCHAR(1024) NOT NULL,
    created_at DATETIME(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),

    INDEX idx_dl_timestamp_utc (timestamp_utc)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
