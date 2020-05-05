﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using tasktServer.Models;

namespace tasktServer.Migrations
{
    [DbContext(typeof(TasktDatabaseContext))]
    [Migration("20190128013720_scriptFriendlyName")]
    partial class ScriptFriendlyName
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("tasktServer.Models.PublishedScript", b =>
                {
                    b.Property<Guid>("PublishedScriptID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FriendlyName");

                    b.Property<DateTime>("PublishedOn");

                    b.Property<string>("ScriptData");

                    b.Property<int>("ScriptType");

                    b.Property<Guid>("WorkerID");

                    b.HasKey("PublishedScriptID");

                    b.ToTable("PublishedScripts");
                });

            modelBuilder.Entity("tasktServer.Models.Task", b =>
                {
                    b.Property<Guid>("TaskID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("IPAddress");

                    b.Property<string>("MachineName");

                    b.Property<string>("Status");

                    b.Property<DateTime>("TaskFinished");

                    b.Property<string>("TaskName");

                    b.Property<DateTime>("TaskStarted");

                    b.Property<string>("UserName");

                    b.Property<Guid>("WorkerID");

                    b.HasKey("TaskID");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("tasktServer.Models.Worker", b =>
                {
                    b.Property<Guid>("WorkerID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("LastCheckIn");

                    b.Property<string>("MachineName");

                    b.Property<int>("Status");

                    b.Property<string>("UserName");

                    b.HasKey("WorkerID");

                    b.ToTable("Workers");
                });
#pragma warning restore 612, 618
        }
    }
}
