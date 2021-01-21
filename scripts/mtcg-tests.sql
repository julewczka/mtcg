/***** user *****/
select * from "user";
select * from "user" where username = 'admin';

update "user" set token = 'admin-mtcgToken' where username = 'admin';

/***** card *****/
select * from card;
select * from card join pack_cards pc on card.uuid = pc.card_uuid where pack_uuid = '24e7116c-819e-40ea-b244-f3017bab29ea';
delete from card;

/***** packages *****/
select * from packages;
select * from pack_cards;

delete from pack_cards;
delete from packages;

/***** stack *****/
select * from stack;
select * from stack_cards;

insert into stack(user_uuid) values ('4fa8fd0f-5ed1-4ece-a761-54366aa822eb');
insert into stack_cards(stack_uuid, card_uuid)  values ('f97ca1b3-79d0-46cb-8ccb-43d1417f0436', 'dfdd758f-649c-40f9-ba3a-8657f4b3439f');

delete from stack_cards;
delete from stack;

/***** logins *****/
select * from logins;