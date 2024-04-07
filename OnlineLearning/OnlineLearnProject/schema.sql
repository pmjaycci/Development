CREATE TABLE IF NOT EXISTS accounts (
    user_id VARCHAR(20) PRIMARY KEY NOT NULL,
    pw VARCHAR(20) NOT NULL,
    cash INTEGER DEFAULT 0
);
CREATE TABLE IF NOT EXISTS online_learn (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id VARCHAR(10) NOT NULL,
    price INTEGER NOT NULL,
    title VARCHAR(45) NOT NULL,
    contents TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS online_learn_comment (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    online_learn_id INTEGER NOT NULL,
    user_id VARCHAR(10) NOT NULL,
    comment TEXT NOT NULL,
    CONSTRAINT online_learn_comment_fk FOREIGN KEY(online_learn_id) 
    REFERENCES online_learn(id)
);

CREATE TABLE IF NOT EXISTS online_learn_reply (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    comment_id INTEGER NOT NULL,
    user_id VARCHAR(10) NOT NULL,
    comment TEXT NOT NULL,
    CONSTRAINT online_learn_reply_fk FOREIGN KEY(comment_id) 
    REFERENCES online_learn_comment(online_learn_id)
);

CREATE TABLE IF NOT EXISTS buy_online_learn (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id VARCHAR(10) NOT NULL,
    online_learn_id INTEGER NOT NULL,
    amount INTEGER NOT NULL,
    card_name VARCHAR(30) NOT NULL,
    receipt_url TEXT NOT NULL,
    created_at TEXT,
    CONSTRAINT buy_online_learn_fk FOREIGN KEY(online_learn_id)
    REFERENCES online_learn(id)
);

CREATE TABLE IF NOT EXISTS sell_online_learn (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id VARCHAR(10) NOT NULL,
    online_learn_id INTEGER NOT NULL,
    created_at TEXT,
    CONSTRAINT sell_online_learn_fk FOREIGN KEY(online_learn_id)
    REFERENCES online_learn(id)
);