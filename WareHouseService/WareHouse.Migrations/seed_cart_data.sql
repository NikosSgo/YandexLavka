-- Вставляем данные в корзину (cart_items)
-- Этот скрипт можно выполнить отдельно после создания таблицы cart_items

-- Удаляем существующие данные (опционально, если нужно перезаписать)
-- DELETE FROM cart_items;

INSERT INTO cart_items (id, customer_id, product_id, quantity, created_at, updated_at) VALUES

-- Корзина для customer-001 (3 товара)
('aaaaaaaa-1111-1111-1111-111111111111', 'customer-001', '11111111-1111-1111-1111-111111111111', 2, NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours'),
('aaaaaaaa-1111-1111-1111-111111111112', 'customer-001', '77777777-7777-7777-7777-777777777777', 1, NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours'),
('aaaaaaaa-1111-1111-1111-111111111113', 'customer-001', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 3, NOW() - INTERVAL '1 hour 30 minutes', NOW() - INTERVAL '1 hour 30 minutes'),

-- Корзина для customer-002 (4 товара)
('bbbbbbbb-2222-2222-2222-222222222221', 'customer-002', '22222222-2222-2222-2222-222222222222', 3, NOW() - INTERVAL '3 hours', NOW() - INTERVAL '3 hours'),
('bbbbbbbb-2222-2222-2222-222222222222', 'customer-002', '88888888-8888-8888-8888-888888888888', 4, NOW() - INTERVAL '3 hours', NOW() - INTERVAL '3 hours'),
('bbbbbbbb-2222-2222-2222-222222222223', 'customer-002', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 1, NOW() - INTERVAL '2 hours 30 minutes', NOW() - INTERVAL '2 hours 30 minutes'),
('bbbbbbbb-2222-2222-2222-222222222224', 'customer-002', '55555555-6666-7777-8888-999999999999', 2, NOW() - INTERVAL '2 hours', NOW() - INTERVAL '2 hours'),

-- Корзина для customer-003 (2 товара)
('cccccccc-3333-3333-3333-333333333331', 'customer-003', '33333333-3333-3333-3333-333333333333', 1, NOW() - INTERVAL '1 hour', NOW() - INTERVAL '1 hour'),
('cccccccc-3333-3333-3333-333333333332', 'customer-003', '99999999-9999-9999-9999-999999999999', 2, NOW() - INTERVAL '1 hour', NOW() - INTERVAL '1 hour'),

-- Корзина для customer-004 (5 товаров)
('dddddddd-4444-4444-4444-444444444441', 'customer-004', '44444444-4444-4444-4444-444444444444', 1, NOW() - INTERVAL '4 hours', NOW() - INTERVAL '4 hours'),
('dddddddd-4444-4444-4444-444444444442', 'customer-004', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 6, NOW() - INTERVAL '4 hours', NOW() - INTERVAL '4 hours'),
('dddddddd-4444-4444-4444-444444444443', 'customer-004', '11111111-2222-3333-4444-555555555555', 2, NOW() - INTERVAL '3 hours 30 minutes', NOW() - INTERVAL '3 hours 30 minutes'),
('dddddddd-4444-4444-4444-444444444444', 'customer-004', '77777777-8888-9999-aaaa-bbbbbbbbbbbb', 3, NOW() - INTERVAL '3 hours', NOW() - INTERVAL '3 hours'),
('dddddddd-4444-4444-4444-444444444445', 'customer-004', '99999999-aaaa-bbbb-cccc-dddddddddddd', 1, NOW() - INTERVAL '2 hours 30 minutes', NOW() - INTERVAL '2 hours 30 minutes'),

-- Корзина для customer-005 (3 товара)
('eeeeeeee-5555-5555-5555-555555555551', 'customer-005', '55555555-5555-5555-5555-555555555555', 2, NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes'),
('eeeeeeee-5555-5555-5555-555555555552', 'customer-005', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 4, NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes'),
('eeeeeeee-5555-5555-5555-555555555553', 'customer-005', '22222222-3333-4444-5555-666666666666', 3, NOW() - INTERVAL '20 minutes', NOW() - INTERVAL '20 minutes'),

-- Корзина для customer-006 (2 товара)
('ffffffff-6666-6666-6666-666666666661', 'customer-006', '66666666-6666-6666-6666-666666666666', 1, NOW() - INTERVAL '15 minutes', NOW() - INTERVAL '15 minutes'),
('ffffffff-6666-6666-6666-666666666662', 'customer-006', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 2, NOW() - INTERVAL '10 minutes', NOW() - INTERVAL '10 minutes'),

-- Корзина для customer-007 (4 товара)
('11111111-7777-7777-7777-777777777771', 'customer-007', '77777777-7777-7777-7777-777777777777', 2, NOW() - INTERVAL '45 minutes', NOW() - INTERVAL '45 minutes'),
('11111111-7777-7777-7777-777777777772', 'customer-007', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 4, NOW() - INTERVAL '45 minutes', NOW() - INTERVAL '45 minutes'),
('11111111-7777-7777-7777-777777777773', 'customer-007', '44444444-5555-6666-7777-888888888888', 1, NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes'),
('11111111-7777-7777-7777-777777777774', 'customer-007', 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee', 1, NOW() - INTERVAL '20 minutes', NOW() - INTERVAL '20 minutes'),

-- Корзина для customer-008 (3 товара)
('22222222-8888-8888-8888-888888888881', 'customer-008', '88888888-8888-8888-8888-888888888888', 3, NOW() - INTERVAL '1 hour', NOW() - INTERVAL '1 hour'),
('22222222-8888-8888-8888-888888888882', 'customer-008', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 2, NOW() - INTERVAL '1 hour', NOW() - INTERVAL '1 hour'),
('22222222-8888-8888-8888-888888888883', 'customer-008', '55555555-6666-7777-8888-999999999999', 2, NOW() - INTERVAL '50 minutes', NOW() - INTERVAL '50 minutes'),

-- Корзина для customer-009 (2 товара)
('33333333-9999-9999-9999-999999999991', 'customer-009', '99999999-9999-9999-9999-999999999999', 1, NOW() - INTERVAL '25 minutes', NOW() - INTERVAL '25 minutes'),
('33333333-9999-9999-9999-999999999992', 'customer-009', 'ffffffff-ffff-ffff-ffff-ffffffffffff', 5, NOW() - INTERVAL '25 minutes', NOW() - INTERVAL '25 minutes'),

-- Корзина для customer-010 (3 товара)
('44444444-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'customer-010', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 4, NOW() - INTERVAL '5 minutes', NOW() - INTERVAL '5 minutes'),
('44444444-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'customer-010', '11111111-2222-3333-4444-555555555555', 3, NOW() - INTERVAL '5 minutes', NOW() - INTERVAL '5 minutes'),
('44444444-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'customer-010', '77777777-8888-9999-aaaa-bbbbbbbbbbbb', 2, NOW() - INTERVAL '3 minutes', NOW() - INTERVAL '3 minutes')

ON CONFLICT (customer_id, product_id) DO UPDATE 
SET 
    quantity = EXCLUDED.quantity,
    updated_at = NOW();

