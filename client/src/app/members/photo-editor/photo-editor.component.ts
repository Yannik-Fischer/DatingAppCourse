import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from "ng2-file-upload";
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { MembersService } from '../../_services/members.service';
import { Photo } from '../../_models/photo';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {
  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  private baseUrl = environment.apiUrl;

  member = input.required<Member>();
  memberChange = output<Member>();

  uploader?: FileUploader;
  hasBaseDropZoneOver = false;

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = e;
  }

  deletePhoto(photo: Photo) {
    this.memberService.deletePhoto(photo).subscribe({
      next: () => {
        const updatedMember = { ...this.member() };
        updatedMember.photos = updatedMember.photos.filter(x => x.id !== photo.id);
        this.memberChange.emit(updatedMember);
      }
    });
  }

  setMainPhoto(photo: Photo) {
    this.memberService.setMainPhoto(photo).subscribe({
      next: () => this.updateClientPhoto(photo)
    })
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.accountService.currentUser()?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, header) => {
      const photo = JSON.parse(response);
      const updatedMember = { ...this.member() };

      updatedMember.photos.push(photo);

      if (photo.isMain) {
        this.updateClientPhoto(photo);
        // this.memberService.members.update(members => members.map(m => {
        //   if (m.photos.includes(photo)) {
        //     m.photoUrl = photo.url;
        //   }
        //   return m;
        // }));
      } else {
        this.memberChange.emit(updatedMember);
      }
    }
  }

  updateClientPhoto(photo: Photo) {
    const user = this.accountService.currentUser();
    if (user) {
      user.photoUrl = photo.url;
      this.accountService.setCurrentUser(user);
    }

    const updatedMember = { ...this.member() };
    updatedMember.photoUrl = photo.url;
    updatedMember.photos.forEach(p => {
      if (p.isMain) {
        p.isMain = false;
      }
      if (p.id === photo.id) {
        p.isMain = true;
      }
    })

    this.memberChange.emit(updatedMember);
  }
}
