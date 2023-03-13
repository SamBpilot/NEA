//
//  NEAApp.swift
//  NEA
//
//  Created by Sam Blewitt on 09/09/2022.
//

import SwiftUI
import Network

@main
struct NEAApp: App {
    @StateObject var serverObject = ServerObject()
    
    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(serverObject)
        }
    }
}
