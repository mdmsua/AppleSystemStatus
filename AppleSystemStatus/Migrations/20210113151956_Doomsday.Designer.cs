﻿// <auto-generated />
using System;
using AppleSystemStatus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AppleSystemStatus.Migrations
{
    [DbContext(typeof(AppleSystemStatusDbContext))]
    [Migration("20210113151956_Doomsday")]
    partial class Doomsday
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AppleSystemStatus.Entities.Country", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("AppleSystemStatus.Entities.Event", b =>
                {
                    b.Property<Guid>("ServiceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("EpochStartDate")
                        .HasColumnType("bigint");

                    b.Property<string>("AffectedServices")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DatePosted")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EndDate")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("EpochEndDate")
                        .HasColumnType("bigint");

                    b.Property<int>("EventStatus")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("MessageId")
                        .HasColumnType("bigint");

                    b.Property<string>("StartDate")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StatusType")
                        .HasColumnType("int");

                    b.Property<string>("UsersAffected")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ServiceId", "EpochStartDate");

                    b.HasIndex("EpochEndDate");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("AppleSystemStatus.Entities.Service", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CountryId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CountryId");

                    b.HasIndex("Name");

                    b.ToTable("Services");
                });

            modelBuilder.Entity("AppleSystemStatus.Entities.Event", b =>
                {
                    b.HasOne("AppleSystemStatus.Entities.Service", "Service")
                        .WithMany("Events")
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AppleSystemStatus.Entities.Service", b =>
                {
                    b.HasOne("AppleSystemStatus.Entities.Country", "Country")
                        .WithMany("Services")
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
