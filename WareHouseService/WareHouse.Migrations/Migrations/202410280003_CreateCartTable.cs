using FluentMigrator;

namespace WareHouse.Migrations.Migrations;

[Migration(202410280003)]
public class CreateCartTable : Migration
{
    public override void Up()
    {
        Execute.Sql(@"
            CREATE TABLE cart_items (
                id UUID PRIMARY KEY,
                customer_id VARCHAR(100) NOT NULL,
                product_id UUID NOT NULL,
                quantity INTEGER NOT NULL,
                created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                CONSTRAINT fk_cart_items_products 
                    FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE,
                CONSTRAINT chk_cart_items_quantity 
                    CHECK (quantity > 0),
                CONSTRAINT uk_cart_items_customer_product 
                    UNIQUE (customer_id, product_id)
            );
        ");

        // Создаем индексы для производительности
        Execute.Sql(@"
            CREATE INDEX ix_cart_items_customer_id ON cart_items(customer_id);
            CREATE INDEX ix_cart_items_product_id ON cart_items(product_id);
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            DROP INDEX IF EXISTS ix_cart_items_product_id;
            DROP INDEX IF EXISTS ix_cart_items_customer_id;
            DROP TABLE IF EXISTS cart_items;
        ");
    }
}

