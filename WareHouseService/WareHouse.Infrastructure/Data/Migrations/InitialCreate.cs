using Microsoft.EntityFrameworkCore.Migrations;
using WareHouse.Infrastructure.Data;

#nullable disable

namespace WareHouse.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    picking_started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    picking_completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "picking_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_picker = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    zone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_picking_tasks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "storage_units",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    reserved_quantity = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    zone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    last_restocked = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_storage_units", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "order_lines",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantity_ordered = table.Column<int>(type: "integer", nullable: false),
                    quantity_picked = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_lines", x => new { x.order_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_order_lines_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "picking_items",
                columns: table => new
                {
                    picking_task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    storage_location = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_picking_items", x => new { x.picking_task_id, x.product_id });
                    table.ForeignKey(
                        name: "fk_picking_items_picking_tasks_picking_task_id",
                        column: x => x.picking_task_id,
                        principalTable: "picking_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_picking_tasks_assigned_picker",
                table: "picking_tasks",
                column: "assigned_picker");

            migrationBuilder.CreateIndex(
                name: "ix_picking_tasks_order_id",
                table: "picking_tasks",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_picking_tasks_status",
                table: "picking_tasks",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_picking_tasks_zone",
                table: "picking_tasks",
                column: "zone");

            migrationBuilder.CreateIndex(
                name: "ix_storage_units_location",
                table: "storage_units",
                column: "location");

            migrationBuilder.CreateIndex(
                name: "ix_storage_units_product_id",
                table: "storage_units",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_storage_units_product_id_location",
                table: "storage_units",
                columns: new[] { "product_id", "location" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_storage_units_zone",
                table: "storage_units",
                column: "zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_lines");

            migrationBuilder.DropTable(
                name: "picking_items");

            migrationBuilder.DropTable(
                name: "storage_units");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "picking_tasks");
        }
    }
}