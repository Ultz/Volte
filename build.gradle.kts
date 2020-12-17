import org.jetbrains.kotlin.gradle.tasks.KotlinCompile

plugins {
    kotlin("jvm") version "1.4.20"
    application
}

group = "volte"
version = "4.0.0"

repositories {
    mavenCentral()
    jcenter()
}

dependencies {
    implementation("ch.qos.logback:logback-classic:1.2.3")
    implementation("net.dv8tion:JDA:4.2.0_222") {
        exclude("opus-java")
    }
    implementation("com.jagrosh:jda-utilities:3.0.1")
    implementation("org.apache.commons:commons-lang3:3.9")
    implementation("commons-io:commons-io:2.6")
    implementation("com.google.code.gson:gson:2.8.6")
    implementation("com.h2database:h2:1.4.200")
    implementation("com.jagrosh:EasySQL:0.3")
}

tasks.withType<KotlinCompile>() {
    kotlinOptions.jvmTarget = "11"
}

application {
    mainClassName = "MainKt"
}