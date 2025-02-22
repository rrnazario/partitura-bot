﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;

#nullable disable

namespace TelegramPartHook.Infrastructure.Migrations
{
    [DbContext(typeof(BotContext))]
    [Migration("20220807183541_configCache")]
    partial class configCache
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("TelegramPartHook.Domain.Aggregations.CacheAggregation.SearchCache", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<SheetSearchResult[]>("results")
                        .HasColumnType("jsonb")
                        .HasColumnName("results");

                    b.Property<string>("term")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("term");

                    b.HasKey("id");

                    b.ToTable("search", "public");
                });

            modelBuilder.Entity("TelegramPartHook.Domain.Aggregations.ConfigAggregation.Config", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<int>("MinutesToClean")
                        .HasColumnType("integer")
                        .HasColumnName("minutestoclean");

                    b.Property<DateTime>("NextDateToCacheClear")
                        .HasColumnType("timestamp")
                        .HasColumnName("nextdatetocacheclear");

                    b.Property<DateTime>("NextDateToMonitorRun")
                        .HasColumnType("timestamp")
                        .HasColumnName("nextdatetomonitorrun");

                    b.Property<DateTime>("NextDateSearchOnInstagram")
                    .HasColumnType("timestamp")
                    .HasColumnName("nextdatesearchoninstagram");

                    b.HasKey("id");

                    b.ToTable("config", "public");
                });

            modelBuilder.Entity("TelegramPartHook.Domain.Aggregations.UserAggregation.User", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("id"));

                    b.Property<string>("culture")
                        .HasColumnType("text")
                        .HasDefaultValue("en")
                        .HasColumnName("culture");

                    b.Property<string>("fullname")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("fullname");

                    b.Property<bool>("isvip")
                        .HasColumnType("boolean")
                        .HasDefaultValue(false)
                        .HasColumnName("isvip");

                    b.Property<string>("lastsearchdate")
                        .HasColumnType("text")
                        .HasDefaultValue("07/08/2022 20:35:41")
                        .HasColumnName("lastsearchdate");

                    b.Property<int>("searchescount")
                        .HasColumnType("integer")
                        .HasColumnName("searchescount");

                    b.Property<string>("searchesfailed")
                        .HasColumnType("text")
                        .HasDefaultValue("")
                        .HasColumnName("searchesfailed");

                    b.Property<string>("searchesok")
                        .HasColumnType("text")
                        .HasDefaultValue("")
                        .HasColumnName("searchesok");

                    b.Property<string>("searchesscheduled")
                        .HasColumnType("text")
                        .HasDefaultValue("")
                        .HasColumnName("searchesscheduled");

                    b.Property<string>("telegramid")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("telegramid");

                    b.Property<bool>("unsubscribe")
                        .HasColumnType("boolean")
                        .HasColumnName("unsubscribe");

                    b.HasKey("id");

                    b.ToTable("client", "public");
                });
#pragma warning restore 612, 618
        }
    }
}
