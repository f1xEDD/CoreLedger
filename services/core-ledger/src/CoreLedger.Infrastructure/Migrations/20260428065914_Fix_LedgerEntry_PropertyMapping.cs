using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreLedger.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Fix_LedgerEntry_PropertyMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE public.ledger_entries
                SET entry_id = "EntryId",
                    account_id = "AccountId",
                    transfer_id = "TransferId";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_ledger_entries_transfers_TransferId",
                table: "ledger_entries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ledger_entries",
                table: "ledger_entries");

            migrationBuilder.DropIndex(
                name: "ix_ledger_entries_account",
                table: "ledger_entries");

            migrationBuilder.DropIndex(
                name: "ix_ledger_entries_transfer",
                table: "ledger_entries");

            migrationBuilder.DropColumn(
                name: "EntryId",
                table: "ledger_entries");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ledger_entries");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "ledger_entries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ledger_entries",
                table: "ledger_entries",
                column: "entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_account",
                table: "ledger_entries",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_transfer",
                table: "ledger_entries",
                column: "transfer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ledger_entries_transfers_transfer_id",
                table: "ledger_entries",
                column: "transfer_id",
                principalTable: "transfers",
                principalColumn: "transfer_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ledger_entries_transfers_transfer_id",
                table: "ledger_entries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ledger_entries",
                table: "ledger_entries");

            migrationBuilder.DropIndex(
                name: "ix_ledger_entries_account",
                table: "ledger_entries");

            migrationBuilder.DropIndex(
                name: "ix_ledger_entries_transfer",
                table: "ledger_entries");

            migrationBuilder.AddColumn<Guid>(
                name: "EntryId",
                table: "ledger_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "AccountId",
                table: "ledger_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TransferId",
                table: "ledger_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ledger_entries",
                table: "ledger_entries",
                column: "EntryId");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_account",
                table: "ledger_entries",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "ix_ledger_entries_transfer",
                table: "ledger_entries",
                column: "TransferId");

            migrationBuilder.AddForeignKey(
                name: "FK_ledger_entries_transfers_TransferId",
                table: "ledger_entries",
                column: "TransferId",
                principalTable: "transfers",
                principalColumn: "transfer_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
