//
//  InputView.swift
//  NEA
//
//  Created by Sam Blewitt on 12/09/2022.
//

import SwiftUI
import Network

struct InputView: View {
    @EnvironmentObject var server : ServerObject


    @State private var IP: String = ""
    @State private var isShowingAlert: Bool = false
    @State private var isSegueing: Bool = false
    @State private var errorMessage: String = "An error occured"
    
    var body: some View {
        NavigationView{
            
            VStack{
                Text("Please enter the IP")
                    .font(.title)
                TextField("IP", text: $IP)
                    .textFieldStyle(RoundedBorderTextFieldStyle())
                
                NavigationLink(destination: MainView(), isActive: $isSegueing) {
                    Button {
                        do{
                            if (try self.server.server.initialiseServer(ip: NWEndpoint.Host(self.IP)))
                            {
                                // now to the main view
                                self.isSegueing = true
                            }
                            else
                            {
                                // show alert
                                self.isShowingAlert.toggle()
                                
                            }
                        }
                        catch NEA.invalidIP
                        {
                            self.errorMessage = NEA.invalidIP.description
                            self.isShowingAlert.toggle()
                        }
                        catch NEA.failedToContact
                        {
                            self.errorMessage = NEA.invalidIP.description
                            self.isShowingAlert.toggle()
                        }
                        catch {
                            // something went wrong so send an alert
                            self.errorMessage = NEA.unkownError.description
                            self.isShowingAlert.toggle()
                            
                        }
                    } label : {
                        
                        Label {
                            Text("Submit")
                        } icon: {
                            Image(systemName: "rectangle.portrait.and.arrow.right")
                        }
                            .padding()
                            .background(Color.gray)
                            .foregroundColor(.white)
                            .clipShape(RoundedRectangle(cornerRadius: 40))
                        }
                    
                }
            }
            }
            .alert(errorMessage, isPresented: $isShowingAlert){
                Button("OK", role: .cancel) { }
        }
    }
}

struct InputView_Previews: PreviewProvider {
    static var previews: some View {
        InputView()
    }
}
