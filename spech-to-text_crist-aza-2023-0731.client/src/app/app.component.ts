import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
 // public forecasts: WeatherForecast[] = [];

  audioUrl: string = '';
  selectedFile?: File;
  transcriptionText: string = '';

  constructor(private http: HttpClient) {}

  ngOnInit() {
   // this.getForecasts();
  }

 
  //Evento disparado al seleccionar archivo en el forumulario, capturandolo en al variable selectedFile
  onFileSelected(event: any) {
    //Validar que el audio es un archivo mp3 o wav
    const allowedTypes = ['audio/mpeg', 'audio/wav'];
    const fl: File = event.target.files[0];
    if (!allowedTypes.includes(fl.type)) {
      alert('Solo se permiten archivos .mp3 o .wav');
      return;
    }
    //guardar audio en la variable
    this.selectedFile = event.target.files[0];
    this.audioUrl = this.selectedFile?.name || '';
  }

  convertToText() {
    if (!this.selectedFile) return;
    //Colocar Placeholder, mientras se hace la solicitud
    this.transcriptionText = 'Transcribiendo...';
    //Crear un formData para enviar el archivo
    const formData = new FormData();
    formData.append('audio', this.selectedFile);

    //Consumir el API del backend para la trasncripción y mostrar la trasncripcion en el TextField.
    this.http.post<{ texto: string }>('https://localhost:7190/api/SpechToText/upload', formData)
      .subscribe({
        next: response => {
          console.log('✅ Subido', response);
          this.transcriptionText = response.texto;
        },
        error: err => {
          console.error('❌ Error', err);
          this.transcriptionText = err.message;
        }
      });



    console.log('Procesando archivo o URL:', this.audioUrl || this.selectedFile?.name);

  }

  //Limpiar formulario
  clear() {
    this.audioUrl = '';
    this.selectedFile = undefined;
    this.transcriptionText = '';
  }

  exit() {
    window.close(); 
  }

  title = 'spech-to-text_crist-aza-2023-0731.client';
}
