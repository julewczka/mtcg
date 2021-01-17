CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DROP TABLE pack_cards;
DROP TABLE deck_cards;
DROP TABLE packages;
DROP TABLE deck;
DROP TABLE card;
DROP TABLE "user";

CREATE TABLE "user" (
    uuid UUID PRIMARY KEY DEFAULT uuid_generate_v4(), 
    username TEXT NOT NULL UNIQUE, 
    password TEXT NOT NULL, 
    name TEXT,
    bio TEXT, 
    image TEXT, 
    coins INT DEFAULT 20,
    token TEXT UNIQUE
);

CREATE TABLE card (
  uuid UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
  name TEXT NOT NULL,
  card_type TEXT NOT NULL,
  element_type TEXT NOT NULL,
  damage FLOAT NOT NULL
);

CREATE TABLE deck (
    uuid UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_uuid UUID UNIQUE NOT NULL,
    
    FOREIGN KEY (user_uuid) REFERENCES "user"(uuid)
);

CREATE TABLE deck_cards (
    deck_uuid UUID NOT NULL,
    card_uuid UUID NOT NULL,
    
    PRIMARY KEY (deck_uuid, card_uuid),
    FOREIGN KEY (deck_uuid) REFERENCES deck(uuid),
    FOREIGN KEY (card_uuid) REFERENCES card(uuid)
);

CREATE TABLE packages (
    uuid UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    price INT DEFAULT 5
);

CREATE TABLE pack_cards (
    pack_uuid UUID NOT NULL,
    card_uuid UUID NOT NULL,

    PRIMARY KEY (pack_uuid, card_uuid),
    FOREIGN KEY (pack_uuid) REFERENCES packages(uuid),
    FOREIGN KEY (card_uuid) REFERENCES card(uuid)
);